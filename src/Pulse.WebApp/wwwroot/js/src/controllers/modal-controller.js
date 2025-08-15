import { Controller } from "framework"

export default class ModalController extends Controller {
    connect() {
        let boundClose = this.close.bind(this);

        this.backdrop.addEventListener('click', evt => {
            boundClose();
            evt.stopPropagation();
        });

        this.closeButton.addEventListener('click', boundClose)
        this.subscribe(this.openEvent, this.open.bind(this));
    }

    open() {
        this.context.removeAttribute("hidden");
    }

    close() {
        this.context.setAttribute("hidden", "true");
    }
}