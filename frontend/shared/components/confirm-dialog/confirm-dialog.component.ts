import { Component, ElementRef, effect, input, output, viewChild } from '@angular/core';

@Component({
  selector: 'bams-confirm-dialog',
  standalone: true,
  templateUrl: './confirm-dialog.component.html',
  styleUrl: './confirm-dialog.component.css'
})
export class ConfirmDialogComponent {
  readonly open = input(false);
  readonly title = input('Are you sure?');
  readonly message = input('');
  readonly confirmText = input('Confirm');
  readonly cancelText = input('Keep');
  readonly destructive = input(false);
  readonly confirmed = output<void>();
  readonly dismissed = output<void>();

  private readonly dialog = viewChild<ElementRef<HTMLDialogElement>>('dialog');

  constructor() {
    effect(() => {
      const element = this.dialog()?.nativeElement;
      if (!element) return;

      if (this.open() && !element.open) {
        element.showModal();
      } else if (!this.open() && element.open) {
        element.close();
      }
    });
  }

  onCancel(event: Event): void {
    event.preventDefault();
    this.dismissed.emit();
  }
}