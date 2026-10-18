import { Component, input, output } from '@angular/core';

@Component({
  selector: 'bams-feature-unavailable',
  standalone: true,
  templateUrl: './feature-unavailable.component.html'
})
export class FeatureUnavailableComponent {
  readonly title = input.required<string>();
  readonly dashboardRequested = output<void>();
}
