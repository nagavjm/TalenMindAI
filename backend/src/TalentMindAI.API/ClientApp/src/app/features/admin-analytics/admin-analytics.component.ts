import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { NavShellComponent } from '../../shared/components/nav-shell/nav-shell.component';
import { DashboardService } from '../../core/services/dashboard.service';
import { DashboardStats } from '../../core/models/chat.models';

@Component({
  selector: 'tma-admin-analytics',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatTableModule, NavShellComponent],
  templateUrl: './admin-analytics.component.html',
  styleUrl: './admin-analytics.component.scss'
})
export class AdminAnalyticsComponent implements OnInit {
  stats = signal<DashboardStats | null>(null);
  displayedColumns = ['skill', 'count'];

  constructor(private dashboardService: DashboardService) {}

  ngOnInit(): void {
    this.dashboardService.getStats().subscribe((data) => this.stats.set(data));
  }
}
