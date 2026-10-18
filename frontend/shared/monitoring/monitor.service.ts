import { ErrorHandler, Injectable } from '@angular/core';

export type MonitorSeverity = 'info' | 'warning' | 'error' | 'critical';

@Injectable({ providedIn: 'root' })
export class MonitorService {
  logEvent(
    origin: unknown,
    description: string,
    properties: Record<string, unknown> = {}
  ): void {
    console.info('[BAMS]', {
      type: 'event',
      origin: getOriginName(origin),
      description,
      properties
    });
  }

  logException(
    origin: unknown,
    exception: unknown,
    properties: Record<string, unknown> = {},
    severity: MonitorSeverity = 'error'
  ): void {
    const error = exception instanceof Error ? exception : new Error(String(exception));
    console.error('[BAMS]', {
      type: 'exception',
      severity,
      origin: getOriginName(origin),
      name: error.name,
      message: error.message,
      properties
    });
  }
}

@Injectable()
export class MonitoringErrorHandler implements ErrorHandler {
  constructor(private readonly monitor: MonitorService) {}

  handleError(error: unknown): void {
    this.monitor.logException('Angular', error, {}, 'critical');
  }
}

function getOriginName(origin: unknown): string {
  if (typeof origin === 'string') {
    return origin;
  }

  if (typeof origin === 'function') {
    return origin.name || 'Anonymous';
  }

  if (origin && typeof origin === 'object') {
    return origin.constructor?.name || 'Unknown';
  }

  return 'Unknown';
}