use std::ffi::{c_char, CStr};

use rosu_pp::{Beatmap, Difficulty};
use rosu_pp::osu::{OsuDifficultyAttributes, OsuGradualPerformance, OsuPerformanceAttributes, OsuScoreState};

use crate::structs::{FFIGradualResult, FFIOsuDifficultyAttributes, FFIOsuPerformanceAttributes, FFIOsuScoreState, OsuJudgement};

mod structs;

#[no_mangle]
extern "C" fn calculate_osu_performance(
    difficulty: &FFIOsuDifficultyAttributes,
    state: &FFIOsuScoreState,
    mods: u32,
) -> FFIOsuPerformanceAttributes {
    let difficulty: OsuDifficultyAttributes = difficulty.into();
    let state: OsuScoreState = state.into();

    let performance: OsuPerformanceAttributes = difficulty
        .performance()
        .mods(mods)
        .passed_objects(state.total_hits())
        .state(state)
        .calculate()
        .unwrap();

    (&performance).into()
}

#[no_mangle]
extern "C" fn initialize_osu_performance_gradual(
    map_path: *const c_char,
    mods: u32,
) -> *mut OsuGradualPerformanceState {
    let map_path_bytes = unsafe { CStr::from_ptr(map_path) }.to_bytes();
    let map_path: &str = unsafe { std::str::from_utf8_unchecked(map_path_bytes) };

    let map = Beatmap::from_path(map_path).unwrap(); // TODO: handle errors
    let difficulty = Difficulty::new().mods(mods);
    let performance = OsuGradualPerformance::new(difficulty, &map).unwrap();
    let state = OsuGradualPerformanceState {
        performance,
        score: OsuScoreState::new(),
    };

    Box::into_raw(Box::new(state))
}

#[no_mangle]
extern "C" fn calculate_osu_performance_gradual(
    state: &mut OsuGradualPerformanceState,
    new_judgement: OsuJudgement,
    max_combo: u32,
) -> FFIGradualResult {
    state.score.max_combo = max_combo;

    match new_judgement {
        OsuJudgement::None => {}
        OsuJudgement::Result300 => state.score.n300 += 1,
        OsuJudgement::Result100 => state.score.n100 += 1,
        OsuJudgement::Result50 => state.score.n50 += 1,
        OsuJudgement::ResultMiss => state.score.misses += 1,
    }

    let performance: Option<OsuPerformanceAttributes> = if matches!(new_judgement, OsuJudgement::None) {
        // TODO: fork and add method to not advance gradual difficulty
        panic!("Can't handle OsuJudgement::None");
    } else {
        state.performance.next(state.score.clone())
    };

    match performance {
        Some(attrs) => FFIGradualResult::from_attrs(&attrs, &state.score),
        None => FFIGradualResult::failed(),
    }
}

#[no_mangle]
extern "C" fn dispose_osu_performance_gradual(
    state: *mut OsuGradualPerformanceState,
) {
    drop(unsafe { Box::from_raw(state) });
}

struct OsuGradualPerformanceState {
    performance: OsuGradualPerformance,
    score: OsuScoreState,
}
