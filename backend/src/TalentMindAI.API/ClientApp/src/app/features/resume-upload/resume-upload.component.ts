import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Router } from '@angular/router';
import { NavShellComponent } from '../../shared/components/nav-shell/nav-shell.component';
import { ResumeService } from '../../core/services/resume.service';

@Component({
  selector: 'tma-resume-upload',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule, NavShellComponent],
  templateUrl: './resume-upload.component.html',
  styleUrl: './resume-upload.component.scss'
})
export class ResumeUploadComponent {
  selectedFile = signal<File | null>(null);
  uploading = signal(false);
  errorMessage = signal<string | null>(null);
  isDragOver = signal(false);

  constructor(private resumeService: ResumeService, private router: Router) {}

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.setFile(input.files?.[0] ?? null);
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver.set(true);
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver.set(false);
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver.set(false);
    const file = event.dataTransfer?.files?.[0] ?? null;
    this.setFile(file);
  }

  browseFile(fileInput: HTMLInputElement): void {
    fileInput.click();
  }

  private setFile(file: File | null): void {
    this.selectedFile.set(file);
    this.errorMessage.set(null);
  }

  upload(): void {
    const file = this.selectedFile();
    if (!file) return;

    this.uploading.set(true);
    this.errorMessage.set(null);

    this.resumeService.upload(file).subscribe({
      next: (res) => {
        this.uploading.set(false);
        this.router.navigate(['/resume-analysis', res.resumeId]);
      },
      error: () => {
        this.uploading.set(false);
        this.errorMessage.set('Upload failed. Supported types: PDF, DOCX, JPG, PNG.');
      }
    });
  }
}
