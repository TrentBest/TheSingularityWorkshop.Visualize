import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";

const root = new URL("./", import.meta.url);
const manifest = JSON.parse(await readFile(new URL("renderer-policy.json", root), "utf8"));
const html = await readFile(new URL("index.html", root), "utf8");
const ranks = ["Semantic", "Aggregate", "Distant", "Near", "Interactive"];
const distances = manifest.selectionProbes.map(probe => probe.distance);

assert.equal(manifest.generatedBy, "TheSingularityWorkshop.Renderer.ObserverRepresentationPolicy+RendererComputationMachine");
assert.ok(manifest.horizons.every(horizon => horizon.fsmState === horizon.representation));
assert.equal(manifest.horizons.length, 8);
assert.equal(manifest.selectionMatrix.length, distances.length * ranks.length);
assert.equal(manifest.horizons[7].name, "128 m+");

for (const distance of distances) {
  const distanceHorizon = manifest.horizons.find(horizon => distance <= horizon.maxDistance)
    ?? manifest.horizons.at(-1);

  for (const minimumRepresentation of ranks) {
    const actual = manifest.selectionMatrix.find(entry =>
      entry.distance === distance && entry.minimumRepresentation === minimumRepresentation);
    assert.ok(actual, `Missing decision for ${distance} m / ${minimumRepresentation}`);

    let expected = distanceHorizon;
    if (ranks.indexOf(expected.representation) < ranks.indexOf(minimumRepresentation)) {
      const eligible = manifest.horizons.filter(horizon =>
        ranks.indexOf(horizon.representation) >= ranks.indexOf(minimumRepresentation));
      const bestRank = Math.min(...eligible.map(horizon => ranks.indexOf(horizon.representation)));
      expected = eligible.find(horizon => ranks.indexOf(horizon.representation) === bestRank);
    }

    assert.equal(actual.horizonId, expected.id);
    assert.equal(actual.horizon, expected.name);
    assert.equal(actual.representation, expected.representation);
    assert.equal(actual.fsmState, expected.representation);
    assert.equal(actual.updatesPerSecond, expected.updatesPerSecond);
    assert.equal(actual.frameInterval, Math.max(1, Math.round(manifest.targetFrameRate / expected.updatesPerSecond)));
  }
}

const focus = manifest.selectionMatrix.find(entry =>
  entry.distance === manifest.attentionOverride.distance &&
  entry.minimumRepresentation === manifest.attentionOverride.minimumRepresentation);
assert.equal(focus.horizon, manifest.attentionOverride.horizon);
assert.equal(focus.representation, manifest.attentionOverride.representation);
assert.equal(focus.representation, "Interactive");
assert.equal(focus.fsmState, "Interactive");
assert.equal(manifest.attentionOverride.fsmState, "Interactive");
assert.equal(focus.horizon, "Contact");

assert.ok(html.includes('id="observer-distance"'));
assert.ok(html.includes('id="minimum-representation"'));
assert.ok(html.includes('id="stress-all-horizons"'));
assert.ok(html.includes("rendererPolicy.selectionMatrix"));
assert.ok(html.includes("if(!stressAll&&groupIndex!==activeHorizonIndex)"));
assert.ok(html.includes("const visibleGroups=stressAll?groups:[groups[activeHorizonIndex]]"));
assert.ok(html.includes("Policy skips"));
assert.ok(html.includes('id="dust-budget"'));
assert.ok(html.includes('id="dust-triangles"'));
assert.ok(html.includes("dustBudgetByState=Object.freeze({Semantic:0,Aggregate:64,Distant:256,Near:1024,Interactive:4096})"));
assert.ok(html.includes("pass.draw(dustVertices.length/6,dustCount,0,0)"));
assert.ok(html.includes("rebuildDust()"));


console.log("Renderer policy matrix validated: 45 observer decisions drive selected WebGPU horizon work.");
