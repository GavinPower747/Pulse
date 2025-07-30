interface DispatchOptions {
    target: HTMLElement | Document | Window;
    detail: any;
    prefix?: string;
    bubbles?: boolean;
    cancelable?: boolean;
}

type ControllerAttachment = {
    instance: Controller;
    node: HTMLElement;
}