import { Component, computed, inject, signal, ViewChild, ElementRef, AfterViewInit, effect } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Chart, registerables } from 'chart.js';
import { DashboardApiService, type DashboardData } from '../../services/product-type-api.service';

Chart.register(...registerables);

@Component({
  selector: 'app-dashboard-page',
  imports: [DatePipe],
  templateUrl: './dashboard-page.html',
  styleUrl: './dashboard-page.css'
})
export class DashboardPage implements AfterViewInit {
  private readonly api = inject(DashboardApiService);

  @ViewChild('lineCanvas') lineCanvas!: ElementRef<HTMLCanvasElement>;
  @ViewChild('pieCanvas') pieCanvas!: ElementRef<HTMLCanvasElement>;
  private lineChart?: Chart;
  private pieChart?: Chart;

  protected readonly data = signal<DashboardData | null>(null);
  protected readonly loading = signal(true);

  readonly totalTypes = computed(() => this.data()?.totalTypes ?? 0);
  readonly totalUnits = computed(() => this.data()?.totalUnits ?? 0);
  readonly expiringSoon = computed(() => this.data()?.expiringSoon ?? 0);
  readonly recentMovements = computed(() => this.data()?.recentMovements ?? []);

  private readonly monthlyData = computed(() => this.data()?.monthlyData ?? []);
  private readonly pieData = computed(() => this.data()?.pieData ?? { entries: 0, exits: 0 });

  constructor() {
    this.api.get().subscribe({
      next: d => {
        this.data.set(d);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });

    effect(() => {
      this.monthlyData();
      this.pieData();
      this.renderCharts();
    });
  }

  ngAfterViewInit(): void { this.renderCharts(); }

  private renderCharts(): void {
    if (!this.lineCanvas?.nativeElement || !this.pieCanvas?.nativeElement) return;
    const md = this.monthlyData();
    const pd = this.pieData();
    this.lineChart?.destroy();
    this.pieChart?.destroy();

    this.lineChart = new Chart(this.lineCanvas.nativeElement, {
      type: 'line',
      data: {
        labels: md.map(m => m.label),
        datasets: [
          { label: 'Entradas', data: md.map(m => m.entries), borderColor: '#4ade80', backgroundColor: 'rgba(74,222,128,0.08)', fill: true, tension: 0.3, pointRadius: 4, pointBackgroundColor: '#4ade80' },
          { label: 'Saídas', data: md.map(m => m.exits), borderColor: '#f87171', backgroundColor: 'rgba(248,113,113,0.08)', fill: true, tension: 0.3, pointRadius: 4, pointBackgroundColor: '#f87171' },
        ],
      },
      options: {
        responsive: true, maintainAspectRatio: false,
        plugins: { legend: { position: 'top', labels: { usePointStyle: true, padding: 16, font: { size: 12 }, color: '#94a3b8' } } },
        scales: { y: { beginAtZero: true, grid: { color: '#1e293b' }, ticks: { color: '#64748b' } }, x: { grid: { display: false }, ticks: { color: '#64748b' } } },
      },
    });

    this.pieChart = new Chart(this.pieCanvas.nativeElement, {
      type: 'doughnut',
      data: { labels: ['Entradas', 'Saídas'], datasets: [{ data: [pd.entries, pd.exits], backgroundColor: ['#4ade80', '#f87171'], borderWidth: 0 }] },
      options: {
        responsive: true, maintainAspectRatio: false,
        plugins: { legend: { position: 'bottom', labels: { usePointStyle: true, padding: 16, font: { size: 12 }, color: '#94a3b8' } } },
        cutout: '65%',
      },
    });
  }
}