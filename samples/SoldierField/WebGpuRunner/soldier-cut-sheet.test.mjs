import test from "node:test";
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { deriveCutSheet, findPartRecord } from "./soldier-cut-sheet.mjs";

const sample = {
  partId: "armor.test-plate",
  displayName: "Test Plate",
  revision: "0.1.0",
  maturity: "silhouette-blockout",
  geometry: {
    sourceAssetId: "workshop.soldier.exoskeleton.trooper",
    primitive: "box",
    units: "metres",
    nominal: {
      position: [0, 1, 0],
      dimensions: [0.2, 0.3, 0.04],
      dimensionsMeaning: "size",
      coordinateSystem: "soldier-local:+Y-up,+Z-forward,+X-right"
    },
    tolerance: { status: "to-be-engineered" }
  },
  material: { materialKey: "armor", specificationStatus: "to-be-engineered", specification: "Display palette only." },
  interfaces: [],
  manufacturing: { status: "to-be-engineered" },
  inspection: { status: "not-defined", checks: [] },
  research: { status: "not-started", questions: [], candidateValues: [] },
  traceability: { source: "test", createdFor: "test", lastReviewed: null, openIssues: ["Define material."] }
};

test("derives stable cut-sheet identity and explicit engineering unknowns", () => {
  const sheet = deriveCutSheet(sample);
  assert.equal(sheet.documentType, "workshop-cut-sheet");
  assert.equal(sheet.partId, "armor.test-plate");
  assert.deepEqual(sheet.geometry.dimensions, [0.2, 0.3, 0.04]);
  assert.equal(sheet.geometry.tolerance.status, "to-be-engineered");
  assert.deepEqual(sheet.traceability.openIssues, ["Define material."]);
  assert.match(sheet.geometry.note, /not fabrication-ready/);
});

test("rejects records without a stable identity", () => {
  assert.throws(() => deriveCutSheet({ ...sample, partId: "" }), TypeError);
});

test("finds a record by stable part ID without inventing a fallback", () => {
  const bundle = { records: [sample] };
  assert.equal(findPartRecord(bundle, sample.partId), sample);
  assert.equal(findPartRecord(bundle, "missing.part"), null);
});

test("all semantic part records have unique identities and derive cut sheets", async () => {
  const source = await readFile(new URL("./soldier-part-records.json", import.meta.url), "utf8");
  const bundle = JSON.parse(source);
  assert.equal(bundle.recordCount, 38);
  assert.equal(bundle.records.length, 38);
  const identities = new Set();
  for (const record of bundle.records) {
    assert.ok(!identities.has(record.partId), "duplicate part ID: " + record.partId);
    identities.add(record.partId);
    const sheet = deriveCutSheet(record);
    assert.equal(sheet.partId, record.partId);
    assert.equal(sheet.geometry.units, "metres");
    assert.ok(sheet.traceability.openIssues.length > 0);
  }
});
