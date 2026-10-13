import test from "node:test";
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import {
  createResearchState, requestDemonstration, completeSetup,
  investInCooling, fireDemonstration, resetDemonstration
} from "./soldier-research-simulation.mjs";

const catalog = JSON.parse(await readFile(new URL("./research-demonstrations.json", import.meta.url), "utf8"));
const parts = JSON.parse(await readFile(new URL("./soldier-part-records.json", import.meta.url), "utf8"));
const catalogSchema = JSON.parse(await readFile(new URL("./research-demonstrations.schema.json", import.meta.url), "utf8"));
const trialSchema = JSON.parse(await readFile(new URL("./research-trial.schema.json", import.meta.url), "utf8"));



test("catalog conforms to its declared required fields and effect contract", () => {
  assert.equal(catalog.schemaVersion, "1.0.0");
  assert.equal(catalog.status, catalogSchema.properties.status.const);
  for (const field of catalogSchema.required) assert.ok(Object.hasOwn(catalog, field), "missing catalog field: " + field);
  for (const demo of catalog.demonstrations) {
    for (const field of catalogSchema.$defs.demonstration.required) assert.ok(Object.hasOwn(demo, field), "missing demo field: " + field);
    for (const field of catalogSchema.$defs.demonstration.properties.effects.required) assert.equal(typeof demo.effects[field], "boolean");
  }
});

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
  assert.equal(state.trials.length, 2);
  assert.equal(state.trials[0].coolingTier, 0);
  assert.equal(state.trials[1].coolingTier, 1);
  assert.ok(state.trials[1].barrelHeatC < state.trials[0].barrelHeatC);
  assert.ok(state.log.some(line => line.includes("Nearby trooper")));
});

test("rejects unaffordable requests without changing credits", () => {
  const state = createResearchState(10);
  const next = requestDemonstration(state, catalog, "plate-impact");
  assert.equal(next.lastResult.reason, "insufficient-credits");
  assert.equal(next.credits, 10);
});


test("each trial is a versioned, traceable experiment record with a stable part identity", () => {
  let state = requestDemonstration(createResearchState(), catalog, "plate-impact");
  state = completeSetup(state);
  state = fireDemonstration(state, catalog);
  const trial = state.trials[0];

  for (const field of trialSchema.required) {
    assert.ok(Object.hasOwn(trial, field), "missing trial field: " + field);
  }
  assert.equal(trial.trialId, "trial-0001");
  assert.equal(trial.catalogVersion, catalog.schemaVersion);
  assert.equal(trial.costPaid, 120);
  assert.equal(trial.setupMinutes, 18);
  assert.equal(trial.partId, "torso.sternum-plate");
  assert.equal(trial.damageEventId, trial.damageEvent.eventId);
  assert.equal(trial.damageEvent.partId, trial.partId);
  assert.equal(trial.partCondition.partId, trial.partId);
  for (const field of trialSchema.$defs?.damageEvent?.required || []) {
    assert.ok(Object.hasOwn(trial.damageEvent, field), "missing damage event field: " + field);
  }
});

test("damage belongs to a stable part and accumulates across separate trials", () => {
  let state = requestDemonstration(createResearchState(), catalog, "plate-impact");
  state = completeSetup(state);
  state = fireDemonstration(state, catalog);
  state = resetDemonstration(state);
  state = requestDemonstration(state, catalog, "plate-impact");
  state = completeSetup(state);
  state = fireDemonstration(state, catalog);

  assert.equal(state.damageEvents.length, 2);
  assert.deepEqual(state.damageEvents.map(event => event.hitNumber), [1, 2]);
  assert.notEqual(state.damageEvents[0].eventId, state.damageEvents[1].eventId);
  const condition = state.partConditions["torso.sternum-plate"];
  assert.equal(condition.hitCount, 2);
  assert.ok(condition.integrityRemaining < 1);
  assert.equal(condition.latestDamageEventId, state.damageEvents[1].eventId);
  assert.equal(state.trials[1].partCondition.hitCount, 2);
});

test("a thermal-routing trial changes its own component condition, not the armor plate's damage history", () => {
  let state = requestDemonstration(createResearchState(), catalog, "plate-impact");
  state = completeSetup(state);
  state = fireDemonstration(state, catalog);
  state = resetDemonstration(state);
  state = requestDemonstration(state, catalog, "cooling-iteration");
  state = completeSetup(state);
  state = fireDemonstration(state, catalog);

  assert.equal(state.partConditions["torso.sternum-plate"].hitCount, 1);
  assert.equal(state.damageEvents.length, 1);
  assert.equal(state.lastResult.partId, "pack.heat-exchangers.left");
  assert.equal(state.partConditions["pack.heat-exchangers.left"].thermalCycleCount, 1);
  assert.equal(state.partConditions["pack.heat-exchangers.left"].hitCount, 0);
});
