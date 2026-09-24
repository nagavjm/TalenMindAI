import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'tma-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly authConfig = this.authService.authConfig;
  errorMessage = signal<string | null>(null);
  submitting = signal(false);

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  constructor() {
    this.authService.loadAuthConfig().subscribe();
  }

  submit(): void {
    if (this.form.invalid) return;
    this.errorMessage.set(null);
    this.submitting.set(true);

    this.authService.login(this.form.getRawValue() as { email: string; password: string }).subscribe({
      next: () => this.router.navigate(['/dashboard']),
      error: (err) => {
        this.submitting.set(false);
        this.errorMessage.set(
          err?.status === 403 ? 'Basic authentication is currently disabled.' : 'Invalid email or password.'
        );
      }
    });
  }
}
