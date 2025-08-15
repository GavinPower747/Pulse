export interface DispatchOptions {
    target: HTMLElement | Document | Window;
    detail: any;
    prefix?: string;
    bubbles?: boolean;
    cancelable?: boolean;
}

export type ControllerInitializer = new (...args: any[]) => Controller;