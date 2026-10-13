import test from "node:test";
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import {
  buildPartGeometry,
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


test("a selected component is rendered from its own local geometry", () => {
  const first = buildPartGeometry("torso.sternum-plate");
  const second = buildPartGeometry("torso.sternum-plate");
  assert.ok(first instanceof Float32Array);
  assert.ok(first.length > 0);
  assert.equal(first.length % (SOLDIER_GEOMETRY_LAYOUT.floatsPerVertex * 3), 0);
  assert.deepEqual(first, second);
  assert.ok(first.every(Number.isFinite));
  assert.throws(() => buildPartGeometry("part.does-not-exist"), /Unknown soldier part ID/);
});

test("impact events add stable visible geometry only to their own component", () => {
  const base = buildPartGeometry("torso.sternum-plate");
  const impact = {
    eventId: "damage-0001-torso.sternum-plate",
    partId: "torso.sternum-plate",
    positionNormalized: [0.1, -0.1],
    radiusFraction: 0.12,
    hitNumber: 1
  };
  const damaged = buildPartGeometry("torso.sternum-plate", [impact]);
  const otherPart = buildPartGeometry("pack.heat-exchangers.left", [impact]);

  assert.ok(damaged.length > base.length, "impact overlay should add render triangles");
  assert.equal(otherPart.length, buildPartGeometry("pack.heat-exchangers.left").length,
    "an event for the plate must not alter the heat exchanger geometry");
  assert.deepEqual(damaged, buildPartGeometry("torso.sternum-plate", [impact]));
  assert.ok(damaged.every(Number.isFinite));
});


test("soldier shader contract localizes heat effects to simulated muzzle and chassis regions", async () => {
  const html = await readFile(new URL("./index.html", import.meta.url), "utf8");
  assert.match(html, /@group\(0\) @binding\(1\) var<uniform> thermal: T/);
  assert.match(html, /let chassisZone=/);
  assert.match(html, /let barrelZone=/);
  assert.match(html, /thermalGlow/);
  assert.match(html, /thermal\.chassis/);
  assert.match(html, /thermal\.barrel/);
  assert.match(html, /getCompilationInfo/);
  assert.match(html, /render-pipeline validation failed/);
  assert.match(html, /function playFiringAudioCue/);
  assert.match(html, /breath intake and exertion grunt cue played/);
  assert.match(html, /Web Audio unavailable; sound cue skipped/);
});
