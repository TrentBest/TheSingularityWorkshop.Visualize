/**
 * Deterministic gameplay-state helpers for the fictional research-laboratory loop.
 * All prices, timings, hit statistics, and thermal values are simulation parameters,
 * not measured engineering results.
 */
export const RESEARCH_CREDITS_START = 1000;
export const COOLING_RESEARCH_COST = 300;

export function createResearchState(credits = RESEARCH_CREDITS_START) {
  return {
    credits,
    coolingTier: 0,
    selectedDemonstrationId: "plate-impact",
    phase: "catalog",
    activeDemonstrationId: null,
    setupMinutesRemaining: 0,
    shotCount: 0,
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
    lastResult: { ok: true, coolingTier },
    log: [...state.log, `Cooling research funded: Prototype ${String.fromCharCode(65 + coolingTier)}. New heat-routing hypothesis recorded for the next range trial.`]
  };
}

export function fireDemonstration(state, catalog) {
  if (state.phase !== "ready") {
    return { ...state, lastResult: { ok: false, reason: "range-not-ready" } };
  }
  const demo = catalog.demonstrations.find(item => item.id === state.activeDemonstrationId);
  if (!demo) return { ...state, lastResult: { ok: false, reason: "unknown-demonstration" } };

  const shotCount = state.shotCount + (demo.effects.impact ? 1 : 0);
  const baseBarrelHeatC = demo.effects.barrelHeat ? 760 : 0;
  const heatTransfer = Math.min(0.78, 0.18 + state.coolingTier * 0.2);
  const barrelHeatC = demo.effects.barrelHeat ? Math.round(baseBarrelHeatC * (1 - state.coolingTier * 0.12)) : 0;
  const chassisHeatC = demo.effects.armorHeat ? Math.round(110 + heatTransfer * 120) : 0;
  const result = {
    ok: true,
    demonstrationId: demo.id,
    partId: demo.partId,
    impact: Boolean(demo.effects.impact),
    penetration: Boolean(demo.effects.penetration),
    barrelHeatC,
    chassisHeatC,
    heatTransfer,
    coolingTier: state.coolingTier,
    valuesAreFictional: true
  };
  const log = [...state.log];
  if (demo.effects.impact) {
    log.push("PROTOTYPE DISCHARGE. The controller kicks against your palm; the barrel blooms red, and the sternum plate spits sparks around a blackened impact crater.");
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
