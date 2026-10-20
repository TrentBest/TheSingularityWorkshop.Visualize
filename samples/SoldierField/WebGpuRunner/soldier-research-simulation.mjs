/**
 * Deterministic gameplay-state helpers for the fictional research-laboratory loop.
 * Experiment records and damage events retain stable part identity across trials.
 * Prices, timings, hit statistics, damage, and thermal values are simulation data,
 * not measured engineering results or fabrication specifications.
 */
export const RESEARCH_CREDITS_START = 1000;
export const COOLING_RESEARCH_COST = 300;

const trialIdFor = number => `trial-${String(number).padStart(4, "0")}`;

export function createResearchState(credits = RESEARCH_CREDITS_START) {
  return {
    credits,
    coolingTier: 0,
    selectedDemonstrationId: "plate-impact",
    phase: "catalog",
    activeDemonstrationId: null,
    setupMinutesRemaining: 0,
    shotCount: 0,
    trials: [],
    damageEvents: [],
    partConditions: {},
    lastResult: null,
    log: ["Head scientist: “The range is yours, sire. Which question shall we put to the machine?”"]
  };
}

export function requestDemonstration(state, catalog, demonstrationId) {
  const demo = catalog.demonstrations.find(item => item.id === demonstrationId);
  if (!demo) return { ...state, lastResult: { ok: false, reason: "unknown-demonstration" } };
  if (state.phase === "setup" || state.phase === "ready" || state.phase === "firing") {
    return { ...state, lastResult: { ok: false, reason: "demonstration-already-active" } };
  }
  if (state.credits < demo.cost) {
    return { ...state, lastResult: { ok: false, reason: "insufficient-credits", required: demo.cost } };
  }
  return {
    ...state,
    credits: state.credits - demo.cost,
    selectedDemonstrationId: demo.id,
    activeDemonstrationId: demo.id,
    setupMinutesRemaining: demo.setupMinutes,
    phase: "setup",
    lastResult: { ok: true, cost: demo.cost, setupMinutes: demo.setupMinutes },
    log: [...state.log, `Order accepted: ${demo.title}. ${demo.setupMinutes} minutes of simulated setup queued.`]
  };
}

export function completeSetup(state) {
  if (state.phase !== "setup") return state;
  return {
    ...state,
    phase: "ready",
    setupMinutesRemaining: 0,
    log: [...state.log, "Range interlocks green. The helmet tablet offers the prepared demonstration."]
  };
}

export function investInCooling(state) {
  if (state.phase === "setup" || state.phase === "ready" || state.phase === "firing") {
    return { ...state, lastResult: { ok: false, reason: "range-must-be-idle" } };
  }
  if (state.coolingTier >= 3) {
    return { ...state, lastResult: { ok: false, reason: "research-tier-maxed" } };
  }
  if (state.credits < COOLING_RESEARCH_COST) {
    return { ...state, lastResult: { ok: false, reason: "insufficient-credits", required: COOLING_RESEARCH_COST } };
  }
  const coolingTier = state.coolingTier + 1;
  return {
    ...state,
    credits: state.credits - COOLING_RESEARCH_COST,
    coolingTier,
    phase: "catalog",
    lastResult: null,
    activeDemonstrationId: null,
    log: [...state.log, `Cooling research funded: Prototype ${String.fromCharCode(65 + coolingTier)}. New heat-routing hypothesis recorded for the next range trial.`]
  };
}

function nextDamageEvent(demo, trialNumber, priorCondition, trialId) {
  if (!demo.effects.impact) return null;
  const hitNumber = priorCondition.hitCount + 1;
  const integrityLossFraction = demo.effects.penetration ? 0.34 : 0.14;
  return {
    schemaVersion: "1.0.0",
    eventId: `damage-${String(trialNumber).padStart(4, "0")}-${demo.partId}`,
    trialId,
    partId: demo.partId,
    hitNumber,
    positionNormalized: [
      ((trialNumber * 37) % 61) / 100 - 0.30,
      ((trialNumber * 29) % 51) / 100 - 0.25
    ],
    radiusFraction: 0.095 + (trialNumber % 3) * 0.018,
    integrityLossFraction,
    penetration: Boolean(demo.effects.penetration),
    impactHeatIndex: 1,
    valuesAreFictional: true
  };
}

export function fireDemonstration(state, catalog) {
  if (state.phase !== "ready") {
    return { ...state, lastResult: { ok: false, reason: "range-not-ready" } };
  }
  const demo = catalog.demonstrations.find(item => item.id === state.activeDemonstrationId);
  if (!demo) return { ...state, lastResult: { ok: false, reason: "unknown-demonstration" } };

  const trialNumber = state.trials.length + 1;
  const trialId = trialIdFor(trialNumber);
  const priorCondition = state.partConditions[demo.partId] || {
    partId: demo.partId,
    hitCount: 0,
    integrityRemaining: 1,
    accumulatedDamageDepthFraction: 0,
    latestDamageEventId: null,
    thermalCycleCount: 0,
    peakTemperatureC: 20,
    residualTemperatureC: 20
  };
  const shotCount = state.shotCount + (demo.effects.impact ? 1 : 0);
  const baseBarrelHeatC = demo.effects.barrelHeat ? 760 : 0;
  const heatTransfer = Math.min(0.78, 0.18 + state.coolingTier * 0.2);
  const barrelHeatC = demo.effects.barrelHeat ? Math.round(baseBarrelHeatC * (1 - state.coolingTier * 0.12)) : 0;
  const chassisHeatC = demo.effects.armorHeat ? Math.round(110 + heatTransfer * 120) : 0;
  const damageEvent = nextDamageEvent(demo, trialNumber, priorCondition, trialId);
  const condition = {
    ...priorCondition,
    hitCount: priorCondition.hitCount + (damageEvent ? 1 : 0),
    integrityRemaining: damageEvent
      ? Math.max(0, priorCondition.integrityRemaining - damageEvent.integrityLossFraction)
      : priorCondition.integrityRemaining,
    accumulatedDamageDepthFraction: damageEvent
      ? Math.min(0.8, priorCondition.accumulatedDamageDepthFraction + (damageEvent.penetration ? 0.25 : 0.08))
      : priorCondition.accumulatedDamageDepthFraction,
    latestDamageEventId: damageEvent?.eventId || priorCondition.latestDamageEventId,
    thermalCycleCount: priorCondition.thermalCycleCount + (demo.effects.armorHeat ? 1 : 0),
    peakTemperatureC: Math.max(priorCondition.peakTemperatureC, chassisHeatC),
    residualTemperatureC: demo.effects.armorHeat
      ? Math.round(priorCondition.residualTemperatureC * 0.45 + chassisHeatC * 0.55)
      : priorCondition.residualTemperatureC
  };
  const nextPartConditions = { ...state.partConditions, [demo.partId]: condition };
  if (demo.effects.barrelHeat) {
    const priorBarrelCondition = state.partConditions["weapon.muzzle"] || {
      partId: "weapon.muzzle",
      hitCount: 0,
      integrityRemaining: 1,
      accumulatedDamageDepthFraction: 0,
      latestDamageEventId: null,
      thermalCycleCount: 0,
      peakTemperatureC: 20,
      residualTemperatureC: 20
    };
    nextPartConditions["weapon.muzzle"] = {
      ...priorBarrelCondition,
      thermalCycleCount: priorBarrelCondition.thermalCycleCount + 1,
      peakTemperatureC: Math.max(priorBarrelCondition.peakTemperatureC, barrelHeatC),
      residualTemperatureC: Math.round(priorBarrelCondition.residualTemperatureC * 0.45 + barrelHeatC * 0.55)
    };
  }
  const experiment = {
    schemaVersion: "1.0.0",
    trialId,
    trialNumber,
    demonstrationId: demo.id,
    catalogVersion: catalog.schemaVersion,
    partId: demo.partId,
    costPaid: demo.cost,
    setupMinutes: demo.setupMinutes,
    creditsRemaining: state.credits,
    coolingTier: state.coolingTier,
    impact: Boolean(demo.effects.impact),
    penetration: Boolean(demo.effects.penetration),
    damageEventId: damageEvent?.eventId || null,
    damageEvent,
    partCondition: condition,
    barrelHeatC,
    chassisHeatC,
    heatTransfer,
    valuesAreFictional: true
  };
  const result = { ok: true, ...experiment, scientistDialogue: Boolean(demo.effects.scientistDialogue) };
  const log = [...state.log];
  if (damageEvent) {
    log.push(`PROTOTYPE DISCHARGE. The controller kicks against your palm; the barrel blooms red. ${demo.partId} records impact ${damageEvent.hitNumber}; integrity estimate now ${Math.round(condition.integrityRemaining * 100)}%.`);
  }
  if (demo.effects.scientistDialogue) {
    log.push(state.coolingTier === 0
      ? "Head scientist: “As you can see, sire, we still struggle with dissipating the heat. The plate survives the shot, but our thermal path is crude.”"
      : `Head scientist: “Prototype ${String.fromCharCode(65 + state.coolingTier)} moves more of the pulse into the chassis. We have reduced the barrel's peak in this model, but the frame now carries a measurable hot zone.”`);
  }
  if (demo.effects.armorHeat) {
    log.push("A wavering thermal halo ripples around the exoskeleton's heat exchangers. The armor is doing its job—and advertising exactly where the waste heat went.");
  }
  if (state.coolingTier > 0 && demo.effects.barrelHeat) {
    log.push("Nearby trooper, dryly: “New rifle runs cooler now. Only cooks your shoulder instead of your hand.” He laughs and taps the warm chassis plate.");
  }

  return {
    ...state,
    phase: "result",
    shotCount,
    trials: [...state.trials, experiment],
    damageEvents: damageEvent ? [...state.damageEvents, damageEvent] : state.damageEvents,
    partConditions: nextPartConditions,
    lastResult: result,
    log
  };
}

export function resetDemonstration(state) {
  if (state.phase !== "result") return state;
  return {
    ...state,
    phase: "catalog",
    activeDemonstrationId: null,
    setupMinutesRemaining: 0,
    lastResult: null,
    log: [...state.log, "Range cleared. The head scientist returns the helmet tablet to the research catalog."]
  };
}
