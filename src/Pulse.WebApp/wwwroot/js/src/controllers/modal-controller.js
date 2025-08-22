import { Controller } from "framework";

export default class ModalController extends Controller {
  connect() {
    let boundClose = this.close.bind(this);

    this.backdrop.addEventListener("click", (evt) => {
      boundClose();
      evt.stopPropagation();
    });

    this.mainContent.addEventListener("click", (evt) => {
      evt.stopPropagation(); // Prevent click events from propagating to the backdrop
    });

    this.closeButton.addEventListener("click", boundClose);
    this.subscribe(this.openEvent, this.open.bind(this));
  }

  open() {
    this.context.removeAttribute("hidden");
    document.body.classList.add("overflow-hidden");
  }

  close() {
    this.context.setAttribute("hidden", "true");
    document.body.classList.remove("overflow-hidden");
  }
}
