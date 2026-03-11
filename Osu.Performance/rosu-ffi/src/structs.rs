use rosu_pp::osu::{OsuDifficultyAttributes, OsuPerformanceAttributes, OsuScoreState};

#[repr(C)]
pub struct FFIOsuDifficultyAttributes {
    pub stars: f64,
    pub max_combo: u32,
    pub speed_note_count: f64,

    pub approach_rate: f64,
    pub health_rate: f64,

    pub aim_skill: f64,
    pub speed_skill: f64,
    pub flashlight_skill: f64,
    pub slider_skill: f64,

    pub aim_difficult_strain_count: f64,
    pub speed_difficult_strain_count: f64,

    pub circle_count: u32,
    pub slider_count: u32,
    pub spinner_count: u32,
}

#[repr(C)]
pub struct FFIOsuScoreState {
    pub max_combo: u32,
    pub count_300: u32,
    pub count_100: u32,
    pub count_50: u32,
    pub count_0: u32,
}

#[repr(C)]
pub struct FFIOsuPerformanceAttributes {
    pub pp_total: f64,
    pub pp_aim: f64,
    pub pp_speed: f64,
    pub pp_accuracy: f64,
    pub pp_flashlight: f64,
    pub effective_miss_count: f64,
}

#[repr(C)]
pub struct FFIGradualResult {
    pub pp: f64,
    pub pp_aim: f64,
    pub pp_speed: f64,
    pub pp_acc: f64,
    pub pp_flashlight: f64,
    pub effective_miss_count: f64,
    pub aim_difficult_strain_count: f64,
    pub speed_difficult_strain_count: f64,
    pub diff_aim: f64,
    pub misses: u32,
    pub n300: u32,
    pub n100: u32,
    pub n50: u32,
}

#[repr(u8)]
#[allow(unused)]
pub enum OsuJudgement {
    None = 0,
    Result300 = 1,
    Result100 = 2,
    Result50 = 3,
    ResultMiss = 4,
}

impl From<&FFIOsuDifficultyAttributes> for OsuDifficultyAttributes {
    #[inline]
    fn from(value: &FFIOsuDifficultyAttributes) -> Self {
        OsuDifficultyAttributes {
            stars: value.stars,
            max_combo: value.max_combo,
            speed_note_count: value.speed_note_count,

            ar: value.approach_rate,
            hp: value.health_rate,
            great_hit_window: 0.0,
            ok_hit_window: 0.0,
            meh_hit_window: 0.0,

            aim: value.aim_skill,
            speed: value.speed_skill,
            flashlight: value.flashlight_skill,
            slider_factor: value.slider_skill,

            aim_difficult_slider_count: 0.0,
            aim_difficult_strain_count: value.aim_difficult_strain_count,
            speed_difficult_strain_count: value.speed_difficult_strain_count,

            n_circles: value.circle_count,
            n_sliders: value.slider_count,
            n_large_ticks: 0,
            n_spinners: value.spinner_count,
        }
    }
}

impl From<&FFIOsuScoreState> for OsuScoreState {
    #[inline]
    fn from(value: &FFIOsuScoreState) -> Self {
        OsuScoreState {
            max_combo: value.max_combo,
            n300: value.count_300,
            n100: value.count_100,
            n50: value.count_50,
            misses: value.count_0,
            ..Default::default()
        }
    }
}

impl From<&OsuPerformanceAttributes> for FFIOsuPerformanceAttributes {
    #[inline]
    fn from(value: &OsuPerformanceAttributes) -> Self {
        FFIOsuPerformanceAttributes {
            pp_total: value.pp,
            pp_aim: value.pp_aim,
            pp_speed: value.pp_speed,
            pp_accuracy: value.pp_acc,
            pp_flashlight: value.pp_flashlight,
            effective_miss_count: value.effective_miss_count,
        }
    }
}

impl FFIGradualResult {
    pub fn from_attrs(attrs: &OsuPerformanceAttributes, score: &OsuScoreState) -> Self {
        FFIGradualResult {
            pp: attrs.pp,
            pp_aim: attrs.pp_aim,
            pp_speed: attrs.pp_speed,
            pp_acc: attrs.pp_acc,
            pp_flashlight: attrs.pp_flashlight,
            effective_miss_count: attrs.effective_miss_count,
            aim_difficult_strain_count: attrs.difficulty.aim_difficult_strain_count,
            speed_difficult_strain_count: attrs.difficulty.speed_difficult_strain_count,
            diff_aim: attrs.difficulty.aim,
            misses: score.misses,
            n300: score.n300,
            n100: score.n100,
            n50: score.n50,
        }
    }

    pub fn failed() -> Self {
        FFIGradualResult {
            pp: -1.0,
            pp_aim: 0.0,
            pp_speed: 0.0,
            pp_acc: 0.0,
            pp_flashlight: 0.0,
            effective_miss_count: 0.0,
            aim_difficult_strain_count: 0.0,
            speed_difficult_strain_count: 0.0,
            diff_aim: 0.0,
            misses: 0,
            n300: 0,
            n100: 0,
            n50: 0,
        }
    }
}
