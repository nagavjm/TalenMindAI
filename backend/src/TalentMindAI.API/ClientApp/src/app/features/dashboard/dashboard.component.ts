import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { NavShellComponent } from '../../shared/components/nav-shell/nav-shell.component';
import { DashboardService } from '../../core/services/dashboard.service';
import { DashboardStats } from '../../core/models/chat.models';

@Component({
  selector: 'tma-dashboard',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatIconModule, NavShellComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  stats = signal<DashboardStats | null>(null);

  constructor(private dashboardService: DashboardService) {}

  ngOnInit(): void {
    this.dashboardService.getStats().subscribe((data) => this.stats.set(data));
  }
}
