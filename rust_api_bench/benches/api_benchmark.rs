// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

//! Comprehensive benchmarks for vello_cpu public API
//!
//! This benchmark suite covers all major public API methods for both
//! single-threaded and multi-threaded (8T) configurations.

use criterion::{black_box, criterion_group, criterion_main, BenchmarkId, Criterion, Throughput};
use vello_cpu::color::palette::css;
use vello_cpu::kurbo::{Affine, BezPath, Cap, Join, Point, Rect, Stroke};
use vello_cpu::peniko::{BlendMode, Compose, Fill, Gradient, Mix};
use vello_cpu::{Level, Pixmap, RenderContext, RenderMode, RenderSettings};

// Standard benchmark size
const WIDTH: u16 = 1920;
const HEIGHT: u16 = 1080;

// Small size for faster benchmarks
const SMALL_WIDTH: u16 = 800;
const SMALL_HEIGHT: u16 = 600;

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
// Path Rendering Benchmarks
// ============================================================================

fn bench_fill_rect(c: &mut Criterion) {
    let mut group = c.benchmark_group("fill_rect");
    group.throughput(Throughput::Elements(1));

    let rect = Rect::from_points((100.0, 100.0), (500.0, 400.0));

    group.bench_function("single_thread", |b| {
        let mut ctx = RenderContext::new_with(SMALL_WIDTH, SMALL_HEIGHT, single_threaded_settings());
        let mut pixmap = Pixmap::new(SMALL_WIDTH, SMALL_HEIGHT);

        b.iter(|| {
            ctx.reset();
            ctx.set_paint(css::MAGENTA);
            ctx.fill_rect(black_box(&rect));
            ctx.flush();
            ctx.render_to_pixmap(&mut pixmap);
        });
    });

    group.bench_function("multi_thread_8T", |b| {
        let mut ctx = RenderContext::new_with(SMALL_WIDTH, SMALL_HEIGHT, multi_threaded_settings());
        let mut pixmap = Pixmap::new(SMALL_WIDTH, SMALL_HEIGHT);

        b.iter(|| {
            ctx.reset();
            ctx.set_paint(css::MAGENTA);
            ctx.fill_rect(black_box(&rect));
            ctx.flush();
            ctx.render_to_pixmap(&mut pixmap);
        });
    });

    group.finish();
}

fn bench_stroke_rect(c: &mut Criterion) {
    let mut group = c.benchmark_group("stroke_rect");
    group.throughput(Throughput::Elements(1));

    let rect = Rect::from_points((100.0, 100.0), (500.0, 400.0));
    let stroke = Stroke {
        width: 5.0,
        join: Join::Miter,
        start_cap: Cap::Round,
        end_cap: Cap::Round,
        ..Default::default()
    };

    group.bench_function("single_thread", |b| {
        let mut ctx = RenderContext::new_with(SMALL_WIDTH, SMALL_HEIGHT, single_threaded_settings());
        let mut pixmap = Pixmap::new(SMALL_WIDTH, SMALL_HEIGHT);

        b.iter(|| {
            ctx.reset();
            ctx.set_paint(css::BLUE);
            ctx.set_stroke(stroke.clone());
            ctx.stroke_rect(black_box(&rect));
            ctx.flush();
            ctx.render_to_pixmap(&mut pixmap);
        });
    });

    group.bench_function("multi_thread_8T", |b| {
        let mut ctx = RenderContext::new_with(SMALL_WIDTH, SMALL_HEIGHT, multi_threaded_settings());
        let mut pixmap = Pixmap::new(SMALL_WIDTH, SMALL_HEIGHT);

        b.iter(|| {
            ctx.reset();
            ctx.set_paint(css::BLUE);
            ctx.set_stroke(stroke.clone());
            ctx.stroke_rect(black_box(&rect));
            ctx.flush();
            ctx.render_to_pixmap(&mut pixmap);
        });
    });

    group.finish();
}

fn bench_fill_path_simple(c: &mut Criterion) {
    let mut group = c.benchmark_group("fill_path_simple");
    group.throughput(Throughput::Elements(1));

    // Simple triangle path
    let mut path = BezPath::new();
    path.move_to((200.0, 100.0));
    path.line_to((400.0, 400.0));
    path.line_to((50.0, 400.0));
    path.close_path();

    group.bench_function("single_thread", |b| {
        let mut ctx = RenderContext::new_with(SMALL_WIDTH, SMALL_HEIGHT, single_threaded_settings());
        let mut pixmap = Pixmap::new(SMALL_WIDTH, SMALL_HEIGHT);

        b.iter(|| {
            ctx.reset();
            ctx.set_paint(css::RED);
            ctx.fill_path(black_box(&path));
            ctx.flush();
            ctx.render_to_pixmap(&mut pixmap);
        });
    });

    group.bench_function("multi_thread_8T", |b| {
        let mut ctx = RenderContext::new_with(SMALL_WIDTH, SMALL_HEIGHT, multi_threaded_settings());
        let mut pixmap = Pixmap::new(SMALL_WIDTH, SMALL_HEIGHT);

        b.iter(|| {
            ctx.reset();
            ctx.set_paint(css::RED);
            ctx.fill_path(black_box(&path));
            ctx.flush();
            ctx.render_to_pixmap(&mut pixmap);
        });
    });

    group.finish();
}

fn bench_fill_path_complex(c: &mut Criterion) {
    let mut group = c.benchmark_group("fill_path_complex");
    group.throughput(Throughput::Elements(1));

    // Complex path with curves
    let mut path = BezPath::new();
    path.move_to((100.0, 100.0));
    path.curve_to((200.0, 50.0), (300.0, 150.0), (400.0, 100.0));
    path.curve_to((450.0, 200.0), (400.0, 300.0), (350.0, 350.0));
    path.curve_to((250.0, 400.0), (150.0, 350.0), (100.0, 250.0));
    path.close_path();

    group.bench_function("single_thread", |b| {
        let mut ctx = RenderContext::new_with(SMALL_WIDTH, SMALL_HEIGHT, single_threaded_settings());
        let mut pixmap = Pixmap::new(SMALL_WIDTH, SMALL_HEIGHT);

        b.iter(|| {
            ctx.reset();
            ctx.set_paint(css::GREEN);
            ctx.fill_path(black_box(&path));
            ctx.flush();
            ctx.render_to_pixmap(&mut pixmap);
        });
    });

    group.bench_function("multi_thread_8T", |b| {
        let mut ctx = RenderContext::new_with(SMALL_WIDTH, SMALL_HEIGHT, multi_threaded_settings());
        let mut pixmap = Pixmap::new(SMALL_WIDTH, SMALL_HEIGHT);

        b.iter(|| {
            ctx.reset();
            ctx.set_paint(css::GREEN);
            ctx.fill_path(black_box(&path));
            ctx.flush();
            ctx.render_to_pixmap(&mut pixmap);
        });
    });

    group.finish();
}

fn bench_stroke_path_complex(c: &mut Criterion) {
    let mut group = c.benchmark_group("stroke_path_complex");
    group.throughput(Throughput::Elements(1));

    // Complex curved path
    let mut path = BezPath::new();
    path.move_to((100.0, 300.0));
    path.curve_to((200.0, 100.0), (400.0, 100.0), (500.0, 300.0));
    path.curve_to((600.0, 500.0), (200.0, 500.0), (300.0, 300.0));

    let stroke = Stroke {
        width: 8.0,
        join: Join::Round,
        start_cap: Cap::Round,
        end_cap: Cap::Round,
        ..Default::default()
    };

    group.bench_function("single_thread", |b| {
        let mut ctx = RenderContext::new_with(SMALL_WIDTH, SMALL_HEIGHT, single_threaded_settings());
        let mut pixmap = Pixmap::new(SMALL_WIDTH, SMALL_HEIGHT);

        b.iter(|| {
            ctx.reset();
            ctx.set_paint(css::PURPLE);
            ctx.set_stroke(stroke.clone());
            ctx.stroke_path(black_box(&path));
            ctx.flush();
            ctx.render_to_pixmap(&mut pixmap);
        });
    });

    group.bench_function("multi_thread_8T", |b| {
        let mut ctx = RenderContext::new_with(SMALL_WIDTH, SMALL_HEIGHT, multi_threaded_settings());
        let mut pixmap = Pixmap::new(SMALL_WIDTH, SMALL_HEIGHT);

        b.iter(|| {
            ctx.reset();
            ctx.set_paint(css::PURPLE);
            ctx.set_stroke(stroke.clone());
            ctx.stroke_path(black_box(&path));
            ctx.flush();
            ctx.render_to_pixmap(&mut pixmap);
        });
    });

    group.finish();
}

// ============================================================================
// Gradient Benchmarks
// ============================================================================

fn bench_linear_gradient(c: &mut Criterion) {
    let mut group = c.benchmark_group("linear_gradient");
    group.throughput(Throughput::Elements(1));

    use vello_cpu::peniko::kurbo::Point as PenikoPoint;
    let gradient = Gradient::new_linear(PenikoPoint::new(0.0, 0.0), PenikoPoint::new(800.0, 600.0))
        .with_stops([
            (0.0, css::RED),
            (0.5, css::YELLOW),
            (1.0, css::BLUE),
        ]);

    let rect = Rect::from_points((50.0, 50.0), (750.0, 550.0));

    group.bench_function("single_thread", |b| {
        let mut ctx = RenderContext::new_with(SMALL_WIDTH, SMALL_HEIGHT, single_threaded_settings());
        let mut pixmap = Pixmap::new(SMALL_WIDTH, SMALL_HEIGHT);

        b.iter(|| {
            ctx.reset();
            ctx.set_paint(black_box(gradient.clone()));
            ctx.fill_rect(&rect);
            ctx.flush();
            ctx.render_to_pixmap(&mut pixmap);
        });
    });

    group.bench_function("multi_thread_8T", |b| {
        let mut ctx = RenderContext::new_with(SMALL_WIDTH, SMALL_HEIGHT, multi_threaded_settings());
        let mut pixmap = Pixmap::new(SMALL_WIDTH, SMALL_HEIGHT);

        b.iter(|| {
            ctx.reset();
            ctx.set_paint(black_box(gradient.clone()));
            ctx.fill_rect(&rect);
            ctx.flush();
            ctx.render_to_pixmap(&mut pixmap);
        });
    });

    group.finish();
}

fn bench_radial_gradient(c: &mut Criterion) {
    let mut group = c.benchmark_group("radial_gradient");
    group.throughput(Throughput::Elements(1));

    use vello_cpu::peniko::kurbo::Point as PenikoPoint;
    let gradient = Gradient::new_radial(PenikoPoint::new(400.0, 300.0), 250.0)
        .with_stops([
            (0.0, css::WHITE),
            (0.5, css::CYAN),
            (1.0, css::NAVY),
        ]);

    let rect = Rect::from_points((100.0, 50.0), (700.0, 550.0));

    group.bench_function("single_thread", |b| {
        let mut ctx = RenderContext::new_with(SMALL_WIDTH, SMALL_HEIGHT, single_threaded_settings());
        let mut pixmap = Pixmap::new(SMALL_WIDTH, SMALL_HEIGHT);

        b.iter(|| {
            ctx.reset();
            ctx.set_paint(black_box(gradient.clone()));
            ctx.fill_rect(&rect);
            ctx.flush();
            ctx.render_to_pixmap(&mut pixmap);
        });
    });

    group.bench_function("multi_thread_8T", |b| {
        let mut ctx = RenderContext::new_with(SMALL_WIDTH, SMALL_HEIGHT, multi_threaded_settings());
        let mut pixmap = Pixmap::new(SMALL_WIDTH, SMALL_HEIGHT);

        b.iter(|| {
            ctx.reset();
            ctx.set_paint(black_box(gradient.clone()));
            ctx.fill_rect(&rect);
            ctx.flush();
            ctx.render_to_pixmap(&mut pixmap);
        });
    });

    group.finish();
}

// ============================================================================
// Transform Benchmarks
// ============================================================================

fn bench_transforms(c: &mut Criterion) {
    let mut group = c.benchmark_group("transforms");
    group.throughput(Throughput::Elements(5)); // 5 rectangles

    let transform = Affine::translate((100.0, 100.0))
        * Affine::rotate(0.785398) // 45 degrees
        * Affine::scale(1.5);

    group.bench_function("single_thread", |b| {
        let mut ctx = RenderContext::new_with(SMALL_WIDTH, SMALL_HEIGHT, single_threaded_settings());
        let mut pixmap = Pixmap::new(SMALL_WIDTH, SMALL_HEIGHT);

        b.iter(|| {
            ctx.reset();
            ctx.set_transform(black_box(transform));
            ctx.set_paint(css::ORANGE);

            // Draw 5 rectangles with transform
            for i in 0..5 {
                let offset = i as f64 * 50.0;
                let rect = Rect::from_points((offset, offset), (offset + 40.0, offset + 40.0));
                ctx.fill_rect(&rect);
            }

            ctx.flush();
            ctx.render_to_pixmap(&mut pixmap);
        });
    });

    group.bench_function("multi_thread_8T", |b| {
        let mut ctx = RenderContext::new_with(SMALL_WIDTH, SMALL_HEIGHT, multi_threaded_settings());
        let mut pixmap = Pixmap::new(SMALL_WIDTH, SMALL_HEIGHT);

        b.iter(|| {
            ctx.reset();
            ctx.set_transform(black_box(transform));
            ctx.set_paint(css::ORANGE);

            // Draw 5 rectangles with transform
            for i in 0..5 {
                let offset = i as f64 * 50.0;
                let rect = Rect::from_points((offset, offset), (offset + 40.0, offset + 40.0));
                ctx.fill_rect(&rect);
            }

            ctx.flush();
            ctx.render_to_pixmap(&mut pixmap);
        });
    });

    group.finish();
}

// ============================================================================
// Blending and Compositing Benchmarks
// ============================================================================

fn bench_blend_modes(c: &mut Criterion) {
    let mut group = c.benchmark_group("blend_modes");
    group.throughput(Throughput::Elements(2)); // 2 layers

    let rect1 = Rect::from_points((100.0, 100.0), (400.0, 300.0));
    let rect2 = Rect::from_points((250.0, 200.0), (550.0, 400.0));

    group.bench_function("single_thread", |b| {
        let mut ctx = RenderContext::new_with(SMALL_WIDTH, SMALL_HEIGHT, single_threaded_settings());
        let mut pixmap = Pixmap::new(SMALL_WIDTH, SMALL_HEIGHT);

        b.iter(|| {
            ctx.reset();

            // Base layer
            ctx.set_paint(css::RED);
            ctx.fill_rect(&rect1);

            // Blend layer
            ctx.push_blend_layer(black_box(BlendMode::new(Mix::Multiply, Compose::SrcOver)));
            ctx.set_paint(css::BLUE);
            ctx.fill_rect(&rect2);
            ctx.pop_layer();

            ctx.flush();
            ctx.render_to_pixmap(&mut pixmap);
        });
    });

    group.bench_function("multi_thread_8T", |b| {
        let mut ctx = RenderContext::new_with(SMALL_WIDTH, SMALL_HEIGHT, multi_threaded_settings());
        let mut pixmap = Pixmap::new(SMALL_WIDTH, SMALL_HEIGHT);

        b.iter(|| {
            ctx.reset();

            // Base layer
            ctx.set_paint(css::RED);
            ctx.fill_rect(&rect1);

            // Blend layer
            ctx.push_blend_layer(black_box(BlendMode::new(Mix::Multiply, Compose::SrcOver)));
            ctx.set_paint(css::BLUE);
            ctx.fill_rect(&rect2);
            ctx.pop_layer();

            ctx.flush();
            ctx.render_to_pixmap(&mut pixmap);
        });
    });

    group.finish();
}

fn bench_opacity_layer(c: &mut Criterion) {
    let mut group = c.benchmark_group("opacity_layer");
    group.throughput(Throughput::Elements(1));

    let rect = Rect::from_points((100.0, 100.0), (500.0, 400.0));

    group.bench_function("single_thread", |b| {
        let mut ctx = RenderContext::new_with(SMALL_WIDTH, SMALL_HEIGHT, single_threaded_settings());
        let mut pixmap = Pixmap::new(SMALL_WIDTH, SMALL_HEIGHT);

        b.iter(|| {
            ctx.reset();
            ctx.push_opacity_layer(black_box(0.5));
            ctx.set_paint(css::GREEN);
            ctx.fill_rect(&rect);
            ctx.pop_layer();
            ctx.flush();
            ctx.render_to_pixmap(&mut pixmap);
        });
    });

    group.bench_function("multi_thread_8T", |b| {
        let mut ctx = RenderContext::new_with(SMALL_WIDTH, SMALL_HEIGHT, multi_threaded_settings());
        let mut pixmap = Pixmap::new(SMALL_WIDTH, SMALL_HEIGHT);

        b.iter(|| {
            ctx.reset();
            ctx.push_opacity_layer(black_box(0.5));
            ctx.set_paint(css::GREEN);
            ctx.fill_rect(&rect);
            ctx.pop_layer();
            ctx.flush();
            ctx.render_to_pixmap(&mut pixmap);
        });
    });

    group.finish();
}

// ============================================================================
// Clipping Benchmarks
// ============================================================================

fn bench_clip_layer(c: &mut Criterion) {
    let mut group = c.benchmark_group("clip_layer");
    group.throughput(Throughput::Elements(1));

    // Clip path (circle)
    let mut clip_path = BezPath::new();
    clip_path.move_to((400.0, 200.0));
    for i in 1..=32 {
        let angle = (i as f64) * std::f64::consts::PI * 2.0 / 32.0;
        let x = 400.0 + 150.0 * angle.cos();
        let y = 300.0 + 150.0 * angle.sin();
        clip_path.line_to((x, y));
    }
    clip_path.close_path();

    let rect = Rect::from_points((200.0, 150.0), (600.0, 450.0));

    group.bench_function("single_thread", |b| {
        let mut ctx = RenderContext::new_with(SMALL_WIDTH, SMALL_HEIGHT, single_threaded_settings());
        let mut pixmap = Pixmap::new(SMALL_WIDTH, SMALL_HEIGHT);

        b.iter(|| {
            ctx.reset();
            ctx.push_clip_layer(black_box(&clip_path));
            ctx.set_paint(css::VIOLET);
            ctx.fill_rect(&rect);
            ctx.pop_layer();
            ctx.flush();
            ctx.render_to_pixmap(&mut pixmap);
        });
    });

    group.bench_function("multi_thread_8T", |b| {
        let mut ctx = RenderContext::new_with(SMALL_WIDTH, SMALL_HEIGHT, multi_threaded_settings());
        let mut pixmap = Pixmap::new(SMALL_WIDTH, SMALL_HEIGHT);

        b.iter(|| {
            ctx.reset();
            ctx.push_clip_layer(black_box(&clip_path));
            ctx.set_paint(css::VIOLET);
            ctx.fill_rect(&rect);
            ctx.pop_layer();
            ctx.flush();
            ctx.render_to_pixmap(&mut pixmap);
        });
    });

    group.finish();
}

// ============================================================================
// Blurred Rounded Rectangle Benchmarks
// ============================================================================

fn bench_blurred_rounded_rect(c: &mut Criterion) {
    let mut group = c.benchmark_group("blurred_rounded_rect");
    group.throughput(Throughput::Elements(1));

    let rect = Rect::from_points((100.0, 100.0), (500.0, 400.0));
    let radius = 20.0;
    let std_dev = 10.0;

    group.bench_function("single_thread", |b| {
        let mut ctx = RenderContext::new_with(SMALL_WIDTH, SMALL_HEIGHT, single_threaded_settings());
        let mut pixmap = Pixmap::new(SMALL_WIDTH, SMALL_HEIGHT);

        b.iter(|| {
            ctx.reset();
            ctx.set_paint(css::TEAL);
            ctx.fill_blurred_rounded_rect(black_box(&rect), black_box(radius), black_box(std_dev));
            ctx.flush();
            ctx.render_to_pixmap(&mut pixmap);
        });
    });

    group.bench_function("multi_thread_8T", |b| {
        let mut ctx = RenderContext::new_with(SMALL_WIDTH, SMALL_HEIGHT, multi_threaded_settings());
        let mut pixmap = Pixmap::new(SMALL_WIDTH, SMALL_HEIGHT);

        b.iter(|| {
            ctx.reset();
            ctx.set_paint(css::TEAL);
            ctx.fill_blurred_rounded_rect(black_box(&rect), black_box(radius), black_box(std_dev));
            ctx.flush();
            ctx.render_to_pixmap(&mut pixmap);
        });
    });

    group.finish();
}

// ============================================================================
// Complex Scene Benchmark
// ============================================================================

fn bench_complex_scene(c: &mut Criterion) {
    let mut group = c.benchmark_group("complex_scene");
    group.throughput(Throughput::Elements(20)); // Multiple elements

    use vello_cpu::peniko::kurbo::Point as PenikoPoint;
    let gradient = Gradient::new_linear(PenikoPoint::new(0.0, 0.0), PenikoPoint::new(800.0, 600.0))
        .with_stops([
            (0.0, css::LIGHT_BLUE),
            (1.0, css::NAVY),
        ]);

    group.bench_function("single_thread", |b| {
        let mut ctx = RenderContext::new_with(SMALL_WIDTH, SMALL_HEIGHT, single_threaded_settings());
        let mut pixmap = Pixmap::new(SMALL_WIDTH, SMALL_HEIGHT);

        b.iter(|| {
            ctx.reset();

            // Background gradient
            ctx.set_paint(gradient.clone());
            ctx.fill_rect(&Rect::from_points((0.0, 0.0), (800.0, 600.0)));

            // Draw multiple shapes
            for i in 0..10 {
                let x = (i as f64) * 70.0 + 50.0;
                let y = 100.0 + ((i % 3) as f64) * 150.0;

                // Rectangle with opacity
                ctx.push_opacity_layer(0.7);
                ctx.set_paint(css::RED);
                ctx.fill_rect(&Rect::from_points((x, y), (x + 50.0, y + 50.0)));
                ctx.pop_layer();

                // Circle (approximated with path)
                let mut circle = BezPath::new();
                let cx = x + 25.0;
                let cy = y + 80.0;
                let r = 20.0;
                circle.move_to((cx + r, cy));
                for j in 1..=16 {
                    let angle = (j as f64) * std::f64::consts::PI * 2.0 / 16.0;
                    circle.line_to((cx + r * angle.cos(), cy + r * angle.sin()));
                }
                circle.close_path();

                ctx.set_paint(css::YELLOW);
                ctx.fill_path(&circle);
            }

            ctx.flush();
            ctx.render_to_pixmap(&mut pixmap);
        });
    });

    group.bench_function("multi_thread_8T", |b| {
        let mut ctx = RenderContext::new_with(SMALL_WIDTH, SMALL_HEIGHT, multi_threaded_settings());
        let mut pixmap = Pixmap::new(SMALL_WIDTH, SMALL_HEIGHT);

        b.iter(|| {
            ctx.reset();

            // Background gradient
            ctx.set_paint(gradient.clone());
            ctx.fill_rect(&Rect::from_points((0.0, 0.0), (800.0, 600.0)));

            // Draw multiple shapes
            for i in 0..10 {
                let x = (i as f64) * 70.0 + 50.0;
                let y = 100.0 + ((i % 3) as f64) * 150.0;

                // Rectangle with opacity
                ctx.push_opacity_layer(0.7);
                ctx.set_paint(css::RED);
                ctx.fill_rect(&Rect::from_points((x, y), (x + 50.0, y + 50.0)));
                ctx.pop_layer();

                // Circle (approximated with path)
                let mut circle = BezPath::new();
                let cx = x + 25.0;
                let cy = y + 80.0;
                let r = 20.0;
                circle.move_to((cx + r, cy));
                for j in 1..=16 {
                    let angle = (j as f64) * std::f64::consts::PI * 2.0 / 16.0;
                    circle.line_to((cx + r * angle.cos(), cy + r * angle.sin()));
                }
                circle.close_path();

                ctx.set_paint(css::YELLOW);
                ctx.fill_path(&circle);
            }

            ctx.flush();
            ctx.render_to_pixmap(&mut pixmap);
        });
    });

    group.finish();
}

// ============================================================================
// Criterion Configuration
// ============================================================================

criterion_group!(
    benches,
    bench_fill_rect,
    bench_stroke_rect,
    bench_fill_path_simple,
    bench_fill_path_complex,
    bench_stroke_path_complex,
    bench_linear_gradient,
    bench_radial_gradient,
    bench_transforms,
    bench_blend_modes,
    bench_opacity_layer,
    bench_clip_layer,
    bench_blurred_rounded_rect,
    bench_complex_scene,
);

criterion_main!(benches);
