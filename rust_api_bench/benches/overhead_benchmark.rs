// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

//! Overhead benchmarks for vello_cpu
//!
//! Measures the cost of fundamental operations:
//! - Context creation (single-threaded and multi-threaded)
//! - Pixmap creation
//! - Flush operation

use criterion::{black_box, criterion_group, criterion_main, Criterion};
use vello_cpu::{Level, Pixmap, RenderContext, RenderMode, RenderSettings};

// Standard benchmark size
const WIDTH: u16 = 800;
const HEIGHT: u16 = 600;

/// Create single-threaded settings
fn single_threaded_settings() -> RenderSettings {
    RenderSettings {
        level: Level::try_detect().unwrap_or(Level::fallback()),
        num_threads: 0, // Single-threaded
        render_mode: RenderMode::OptimizeSpeed,
    }
}

/// Create multi-threaded settings (8 threads)
fn multi_threaded_settings() -> RenderSettings {
    RenderSettings {
        level: Level::try_detect().unwrap_or(Level::fallback()),
        num_threads: 8, // 8 threads
        render_mode: RenderMode::OptimizeSpeed,
    }
}

// ============================================================================
// Context Creation Benchmarks
// ============================================================================

fn bench_context_creation(c: &mut Criterion) {
    let mut group = c.benchmark_group("context_creation");

    group.bench_function("single_thread", |b| {
        b.iter(|| {
            let ctx = RenderContext::new_with(
                black_box(WIDTH),
                black_box(HEIGHT),
                single_threaded_settings(),
            );
            black_box(ctx);
        });
    });

    group.bench_function("multi_thread_8T", |b| {
        b.iter(|| {
            let ctx = RenderContext::new_with(
                black_box(WIDTH),
                black_box(HEIGHT),
                multi_threaded_settings(),
            );
            black_box(ctx);
        });
    });

    group.finish();
}

// ============================================================================
// Pixmap Creation Benchmarks
// ============================================================================

fn bench_pixmap_creation(c: &mut Criterion) {
    let mut group = c.benchmark_group("pixmap_creation");

    group.bench_function("800x600", |b| {
        b.iter(|| {
            let pixmap = Pixmap::new(black_box(WIDTH), black_box(HEIGHT));
            black_box(pixmap);
        });
    });

    group.bench_function("1920x1080", |b| {
        b.iter(|| {
            let pixmap = Pixmap::new(black_box(1920), black_box(1080));
            black_box(pixmap);
        });
    });

    group.bench_function("3840x2160", |b| {
        b.iter(|| {
            let pixmap = Pixmap::new(black_box(3840), black_box(2160));
            black_box(pixmap);
        });
    });

    group.finish();
}

// ============================================================================
// Flush Benchmarks
// ============================================================================

fn bench_flush(c: &mut Criterion) {
    let mut group = c.benchmark_group("flush");

    group.bench_function("single_thread_empty", |b| {
        let mut ctx = RenderContext::new_with(WIDTH, HEIGHT, single_threaded_settings());
        b.iter(|| {
            ctx.reset();
            ctx.flush();
        });
    });

    group.bench_function("multi_thread_8T_empty", |b| {
        let mut ctx = RenderContext::new_with(WIDTH, HEIGHT, multi_threaded_settings());
        b.iter(|| {
            ctx.reset();
            ctx.flush();
        });
    });

    group.bench_function("single_thread_with_rect", |b| {
        use vello_cpu::color::palette::css;
        use vello_cpu::kurbo::Rect;

        let mut ctx = RenderContext::new_with(WIDTH, HEIGHT, single_threaded_settings());
        let rect = Rect::from_points((100.0, 100.0), (500.0, 400.0));

        b.iter(|| {
            ctx.reset();
            ctx.set_paint(css::MAGENTA);
            ctx.fill_rect(&rect);
            ctx.flush();
        });
    });

    group.bench_function("multi_thread_8T_with_rect", |b| {
        use vello_cpu::color::palette::css;
        use vello_cpu::kurbo::Rect;

        let mut ctx = RenderContext::new_with(WIDTH, HEIGHT, multi_threaded_settings());
        let rect = Rect::from_points((100.0, 100.0), (500.0, 400.0));

        b.iter(|| {
            ctx.reset();
            ctx.set_paint(css::MAGENTA);
            ctx.fill_rect(&rect);
            ctx.flush();
        });
    });

    group.finish();
}

// ============================================================================
// Combined Operation Benchmarks
// ============================================================================

fn bench_combined_operations(c: &mut Criterion) {
    let mut group = c.benchmark_group("combined_operations");

    group.bench_function("context_and_pixmap_ST", |b| {
        b.iter(|| {
            let ctx = RenderContext::new_with(
                black_box(WIDTH),
                black_box(HEIGHT),
                single_threaded_settings(),
            );
            let pixmap = Pixmap::new(black_box(WIDTH), black_box(HEIGHT));
            black_box((ctx, pixmap));
        });
    });

    group.bench_function("context_and_pixmap_8T", |b| {
        b.iter(|| {
            let ctx = RenderContext::new_with(
                black_box(WIDTH),
                black_box(HEIGHT),
                multi_threaded_settings(),
            );
            let pixmap = Pixmap::new(black_box(WIDTH), black_box(HEIGHT));
            black_box((ctx, pixmap));
        });
    });

    group.finish();
}

criterion_group!(
    benches,
    bench_context_creation,
    bench_pixmap_creation,
    bench_flush,
    bench_combined_operations
);
criterion_main!(benches);
