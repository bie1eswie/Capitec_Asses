import { Component } from '@angular/core';

// The existing shell remains the rendered view while navigation and access
// checks move to Angular Router. Feature components can replace this outlet
// incrementally without duplicating the current booking UI.
@Component({ selector: 'bams-route-view', standalone: true, template: '' })
export class RouteViewComponent {}
