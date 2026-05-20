import { Component, DestroyRef, inject } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthService } from '../../core/services/auth.service';

@Component({
  standalone: true,
  imports: [ReactiveFormsModule],
  template: `
  <div class="min-h-screen bg-gradient-to-br from-primary-600 to-primary-900 flex items-center justify-center p-4">
    <form [formGroup]="form" (ngSubmit)="submit()" class="bg-white p-6 rounded-xl shadow w-full max-w-md space-y-3">
      <h2 class="text-2xl font-bold">Register</h2>
      <input class="w-full rounded" formControlName="firstName" placeholder="First Name" />
      <input class="w-full rounded" formControlName="lastName" placeholder="Last Name" />
      <input class="w-full rounded" formControlName="email" placeholder="Email" />
      <input class="w-full rounded" formControlName="password" type="password" placeholder="Password" />
      <button class="w-full bg-primary-600 text-white py-2 rounded" [disabled]="form.invalid">Create account</button>
    </form>
  </div>`
})
export class RegisterComponent {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  form = this.fb.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]]
  });

  submit(): void {
    if (this.form.invalid) return;
    this.auth.register(this.form.getRawValue() as { firstName: string; lastName: string; email: string; password: string })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.router.navigateByUrl('/dashboard'));
  }
}
