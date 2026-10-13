import test from "node:test";
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import {
  buildSoldierGeometry,
  SOLDIER_GEOMETRY_LAYOUT,
  SOLDIER_PARTS
} from "./soldier-geometry.js";

test("soldier geometry has 38 unique, stable semantic part IDs", async () => {
  assert.equal(SOLDIER_PARTS.length, 38);
  assert.equal(SOLDIER_GEOMETRY_LAYOUT.partCount, SOLDIER_PARTS.length);

  const ids = SOLDIER_PARTS.map(part => part.id);
  assert.equal(new Set(ids).size, ids.length);
  assert.ok(ids.includes("torso.sternum-plate"));
  for (const id of SOLDIER_GEOMETRY_LAYOUT.shuntEndpointIds) {
    assert.ok(ids.includes(id), `Missing declared shunt endpoint: ${id}`);
  }

  const asset = JSON.parse(await readFile(new URL("./soldier-geometry.json", import.meta.url), "utf8"));
  assert.equal(asset.assetId, "workshop.soldier.exoskeleton.trooper");
  assert.equal(asset.partCount, SOLDIER_PARTS.length);
});

test("generated mesh is deterministic finite triangle-list vertex data", () => {
  const first = buildSoldierGeometry();
  const second = buildSoldierGeometry();

  assert.ok(first instanceof Float32Array);
  assert.ok(first.length > 0);
  assert.equal(first.length % (SOLDIER_GEOMETRY_LAYOUT.floatsPerVertex * 3), 0);
  assert.equal(first.length, second.length);
  assert.deepEqual(first, second);
  assert.ok(first.every(Number.isFinite));

  const stride = SOLDIER_GEOMETRY_LAYOUT.floatsPerVertex;
  for (let i = 0; i < first.length; i += stride) {
    for (let channel = 3; channel < 6; channel++) {
      assert.ok(first[i + channel] >= 0 && first[i + channel] <= 1,
        `Invalid material colour component at float ${i + channel}`);
    }
  }

  assert.equal(SOLDIER_GEOMETRY_LAYOUT.bytesPerVertex, stride * Float32Array.BYTES_PER_ELEMENT);
});
