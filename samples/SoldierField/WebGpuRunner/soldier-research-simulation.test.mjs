import test from "node:test";
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import {
  createResearchState, requestDemonstration, completeSetup,
  investInCooling, fireDemonstration, resetDemonstration
} from "./soldier-research-simulation.mjs";

const catalog = JSON.parse(await readFile(new URL("./research-demonstrations.json", import.meta.url), "utf8"));
const parts = JSON.parse(await readFile(new URL("./soldier-part-records.json", import.meta.url), "utf8"));


test("catalog demonstrations and the most-hit hypothesis reference stable part records", () => {
  const ids = new Set(parts.records.map(part => part.partId));
  assert.equal(catalog.simulationAssumptions.mostFrequentlyHitArmorPartId, "torso.sternum-plate");
  assert.equal(catalog.simulationAssumptions.hitFrequencyStatus, "gameplay-premise-not-measured");
  assert.equal(catalog.simulationAssumptions.hitShare, null);
  for (const demo of catalog.demonstrations) {
    assert.ok(ids.has(demo.partId), "unknown part reference: " + demo.partId);
    assert.ok(demo.cost > 0);
    assert.ok(demo.setupMinutes > 0);
  }
});

test("catalog quotes cost and setup before a demonstration is ordered", () => {
  const state = createResearchState();
  const next = requestDemonstration(state, catalog, "plate-impact");
  assert.equal(next.lastResult.ok, true);
  assert.equal(next.credits, 880);
  assert.equal(next.setupMinutesRemaining, 18);
  assert.equal(next.phase, "setup");
});

test("range cannot fire until simulated setup is completed", () => {
  const state = requestDemonstration(createResearchState(), catalog, "plate-impact");
  assert.equal(fireDemonstration(state, catalog).lastResult.reason, "range-not-ready");
  const ready = completeSetup(state);
  assert.equal(ready.phase, "ready");
  assert.equal(fireDemonstration(ready, catalog).lastResult.impact, true);
});

test("baseline shot records impact, heat and scientist explanation as fictional values", () => {
  let state = requestDemonstration(createResearchState(), catalog, "plate-impact");
  state = completeSetup(state);
  state = fireDemonstration(state, catalog);
  assert.equal(state.lastResult.partId, "torso.sternum-plate");
  assert.equal(state.lastResult.impact, true);
  assert.equal(state.lastResult.valuesAreFictional, true);
  assert.ok(state.lastResult.barrelHeatC > 500);
  assert.ok(state.log.some(line => line.includes("struggle with dissipating the heat")));
});

test("funding cooling research changes the next simulated thermal result and unlocks trooper chatter", () => {
  let state = requestDemonstration(createResearchState(), catalog, "plate-impact");
  state = completeSetup(state);
  state = fireDemonstration(state, catalog);
  const baselineBarrel = state.lastResult.barrelHeatC;
  state = resetDemonstration(state);
  state = investInCooling(state);
  assert.equal(state.coolingTier, 1);
  assert.equal(state.credits, 580);
  state = requestDemonstration(state, catalog, "plate-impact");
  state = completeSetup(state);
  state = fireDemonstration(state, catalog);
  assert.ok(state.lastResult.barrelHeatC < baselineBarrel);
  assert.ok(state.lastResult.chassisHeatC > 0);
  assert.ok(state.log.some(line => line.includes("Nearby trooper")));
});

test("rejects unaffordable requests without changing credits", () => {
  const state = createResearchState(10);
  const next = requestDemonstration(state, catalog, "plate-impact");
  assert.equal(next.lastResult.reason, "insufficient-credits");
  assert.equal(next.credits, 10);
});
