import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { NavShellComponent } from '../../shared/components/nav-shell/nav-shell.component';
import { ResumeService } from '../../core/services/resume.service';
import { ResumeAnalysis } from '../../core/models/resume.models';

@Component({
  selector: 'tma-resume-analysis',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, MatChipsModule, NavShellComponent],
  templateUrl: './resume-analysis.component.html',
  styleUrl: './resume-analysis.component.scss'
})
export class ResumeAnalysisComponent implements OnInit {
  resumeId = '';
  analysis = signal<ResumeAnalysis | null>(null);
  loading = signal(true);

  constructor(private route: ActivatedRoute, private router: Router, private resumeService: ResumeService) {}

  ngOnInit(): void {
    this.resumeId = this.route.snapshot.paramMap.get('id') ?? '';
    this.resumeService.analyze(this.resumeId).subscribe({
      next: (res) => {
        this.analysis.set(res);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  viewCandidate(): void {
    this.router.navigate(['/candidate', this.resumeId]);
  }
}
