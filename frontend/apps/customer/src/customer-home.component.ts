import { Component, output } from '@angular/core';

@Component({
  selector: 'bams-customer-home',
  standalone: true,
  templateUrl: './customer-home.component.html'
})
export class CustomerHomeComponent {
  readonly bookRequested = output<void>();
}
