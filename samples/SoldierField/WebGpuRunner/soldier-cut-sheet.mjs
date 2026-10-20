/**
 * Derives a stable, machine-readable cut sheet from one authoritative part record.
 * Presentation layers may format this object as HTML, PDF, or a laboratory board.
 */
export function deriveCutSheet(record) {
  if (!record || typeof record.partId !== "string" || !record.partId.trim()) {
    throw new TypeError("A part record with a stable partId is required.");
  }
  if (!record.geometry?.nominal || !record.material || !record.traceability) {
    throw new TypeError("The part record is missing required geometry, material, or traceability data.");
  }

  const geometry = record.geometry;
  const nominal = geometry.nominal;
  const vector = values => Array.isArray(values) && values.length === 3
    ? values.map(value => Number.isFinite(value) ? value : null)
    : null;

  return {
    documentType: "workshop-cut-sheet",
    schemaVersion: "1.0.0",
    partId: record.partId,
    displayName: record.displayName || record.partId,
    revision: record.revision || "unrevisioned",
    maturity: record.maturity || "unknown",
    geometry: {
      sourceAssetId: geometry.sourceAssetId || null,
      primitive: geometry.primitive || "unknown",
      units: geometry.units || "unspecified",
      coordinateSystem: nominal.coordinateSystem || "unspecified",
      position: vector(nominal.position),
      dimensions: vector(nominal.dimensions),
      dimensionsMeaning: nominal.dimensionsMeaning || "unspecified",
      tolerance: geometry.tolerance || { status: "to-be-engineered" },
      note: "Nominal render-blockout geometry is not fabrication-ready geometry."
    },
    material: {
      key: record.material.materialKey || "unspecified",
      specificationStatus: record.material.specificationStatus || "to-be-engineered",
      specification: record.material.specification || "No physical material specification supplied.",
      massKg: record.material.massKg ?? null,
      densityKgM3: record.material.densityKgM3 ?? null,
      thermal: record.material.thermal || null,
      electromagnetic: record.material.electromagnetic || null
    },
    interfaces: Array.isArray(record.interfaces) ? record.interfaces : [],
    manufacturing: record.manufacturing || { status: "to-be-engineered" },
    inspection: record.inspection || { status: "not-defined", checks: [] },
    research: record.research || { status: "not-started", questions: [], candidateValues: [] },
    traceability: {
      source: record.traceability.source || "unspecified",
      createdFor: record.traceability.createdFor || "unspecified",
      lastReviewed: record.traceability.lastReviewed ?? null,
      openIssues: Array.isArray(record.traceability.openIssues) ? record.traceability.openIssues : []
    }
  };
}

export function findPartRecord(bundle, partId) {
  if (!Array.isArray(bundle?.records)) return null;
  return bundle.records.find(record => record?.partId === partId) || null;
}
