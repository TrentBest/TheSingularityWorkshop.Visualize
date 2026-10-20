import test from "node:test";
import assert from "node:assert/strict";
import { validateRendererTrace } from "./renderer-trace.mjs";

function validTrace() {
  return {
    SchemaVersion: 1,
    Generator: "TheSingularityWorkshop.Renderer",
    Workload: "array-first-observer-horizon",
    FrameRateHz: 60,
    FrameCount: 2,
    Frames: [
      {
        Frame: 0, TimeMilliseconds: 0, Horizon: "Near", Bucket: "Instances",
        RefreshRequired: true, RefreshAccepted: true,
        ReusedCachedRepresentation: false
      },
      {
        Frame: 1, TimeMilliseconds: 17, Horizon: "Far", Bucket: "Instances",
        RefreshRequired: false, RefreshAccepted: false,
        ReusedCachedRepresentation: true
      }
    ]
  };
}

test("accepts a valid versioned Renderer trace", () => {
  const trace = validTrace();
  assert.equal(validateRendererTrace(trace), trace);
});

test("rejects missing or mismatched frame counts", () => {
  const trace = validTrace();
  trace.FrameCount = 3;
  assert.throws(() => validateRendererTrace(trace), /Unsupported Renderer trace/);
});

test("rejects out-of-order frame indices", () => {
  const trace = validTrace();
  trace.Frames[1].Frame = 2;
  assert.throws(() => validateRendererTrace(trace), /Unsupported Renderer trace/);
});

test("rejects duplicate or decreasing timestamps", () => {
  for (const time of [0, -1]) {
    const trace = validTrace();
    trace.Frames[1].TimeMilliseconds = time;
    assert.throws(() => validateRendererTrace(trace), /Unsupported Renderer trace/);
  }
});

test("rejects unsupported horizons and empty bucket names", () => {
  const badHorizon = validTrace();
  badHorizon.Frames[0].Horizon = "Mid";
  assert.throws(() => validateRendererTrace(badHorizon), /Unsupported Renderer trace/);

  const badBucket = validTrace();
  badBucket.Frames[0].Bucket = "  ";
  assert.throws(() => validateRendererTrace(badBucket), /Unsupported Renderer trace/);
});

test("rejects non-boolean policy flags", () => {
  const trace = validTrace();
  trace.Frames[0].RefreshAccepted = "true";
  assert.throws(() => validateRendererTrace(trace), /Unsupported Renderer trace/);
});

test("rejects accepted refreshes that were not required", () => {
  const trace = validTrace();
  trace.Frames[0].RefreshRequired = false;
  assert.throws(() => validateRendererTrace(trace), /Unsupported Renderer trace/);
});
