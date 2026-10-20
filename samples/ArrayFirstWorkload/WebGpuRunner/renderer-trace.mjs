const TRACE_SCHEMA_VERSION = 1;
const TRACE_WORKLOAD = "array-first-observer-horizon";

const TRACE_ERROR =
  "Unsupported Renderer trace: expected schema v1, matching frame count, ordered frames with strictly increasing non-negative timestamps, Near/Far horizons, non-empty bucket names, and consistent refresh/acceptance fields.";

/**
 * Validate the stable subset of the Renderer trace consumed by the browser workload.
 * Keep this function free of browser APIs so CI can exercise the same contract.
 */
export function validateRendererTrace(trace) {
  if (!trace || typeof trace !== "object" || Array.isArray(trace) ||
      trace.SchemaVersion !== TRACE_SCHEMA_VERSION ||
      trace.Workload !== TRACE_WORKLOAD ||
      !Number.isSafeInteger(trace.FrameCount) || trace.FrameCount <= 0 ||
      !Array.isArray(trace.Frames) || trace.Frames.length === 0 ||
      trace.FrameCount !== trace.Frames.length) {
    throw new Error(TRACE_ERROR);
  }

  let previousTime = -1;
  for (let index = 0; index < trace.Frames.length; index++) {
    const row = trace.Frames[index];
    if (!row || typeof row !== "object" || Array.isArray(row) ||
        row.Frame !== index ||
        !Number.isFinite(row.TimeMilliseconds) ||
        row.TimeMilliseconds < 0 ||
        row.TimeMilliseconds <= previousTime ||
        (row.Horizon !== "Near" && row.Horizon !== "Far") ||
        typeof row.RefreshRequired !== "boolean" ||
        typeof row.RefreshAccepted !== "boolean" ||
        typeof row.ReusedCachedRepresentation !== "boolean" ||
        typeof row.Bucket !== "string" || row.Bucket.trim().length === 0 ||
        (row.RefreshAccepted && !row.RefreshRequired)) {
      throw new Error(TRACE_ERROR);
    }
    previousTime = row.TimeMilliseconds;
  }

  return trace;
}
