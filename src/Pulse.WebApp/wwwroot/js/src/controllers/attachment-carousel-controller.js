import { Controller } from "framework";
import { AttachmentClickedEvent } from "../consts/events.js";

export default class AttachmentCarouselController extends Controller {
  connect() {
    window.addEventListener("resize", this.updateArrows.bind(this));

    this.previousButton.addEventListener("click", this.scroll.bind(this));
    this.nextButton.addEventListener("click", this.scroll.bind(this));
    this.carousel.addEventListener("scroll", this.updateArrows.bind(this));
    this.context.querySelectorAll("img").forEach((img) => {
      img.addEventListener("click", this.publishClickEvent.bind(this, img));
    });
  }

  disconnect() {
    this.previousButton.removeEventListener("click", this.scroll.bind(this));
    this.nextButton.removeEventListener("click", this.scroll.bind(this));
    this.carousel.removeEventListener("scroll", this.updateArrows.bind(this));
  }

  scroll(event) {
    const direction = parseInt(event.currentTarget.dataset.direction, 10);
    const scrollAmount = 140; // Corresponds to image width + gap
    this.carousel.context.scrollLeft += direction * scrollAmount;
  }

  updateArrows() {
    if (!this.checkIfArrowsNeeded()) return;

    const carouselEl = this.carousel.context;
    const leftArrow = this.previousButton.context;
    const rightArrow = this.nextButton.context;

    const canScrollLeft = carouselEl.scrollLeft > 5; // Add a small buffer
    leftArrow.classList.toggle("opacity-50", !canScrollLeft);
    leftArrow.classList.toggle("pointer-events-none", !canScrollLeft);
    leftArrow.classList.toggle("opacity-100", canScrollLeft);

    // Use Math.ceil to handle subpixel values
    const canScrollRight =
      Math.ceil(carouselEl.scrollLeft) <
      carouselEl.scrollWidth - carouselEl.clientWidth - 5;
    rightArrow.classList.toggle("opacity-50", !canScrollRight);
    rightArrow.classList.toggle("pointer-events-none", !canScrollRight);
    rightArrow.classList.toggle("opacity-100", canScrollRight);
  }

  checkIfArrowsNeeded() {
    const carouselEl = this.carousel.context;
    if (!carouselEl) return false;

    const needsScrolling = carouselEl.scrollWidth > carouselEl.clientWidth;

    this.previousButton.context.hidden = !needsScrolling;
    this.nextButton.context.hidden = !needsScrolling;
    carouselEl.classList.toggle("px-10", needsScrolling);

    return needsScrolling;
  }

  publishClickEvent(image) {
    this.dispatch(AttachmentClickedEvent.type, {
      bubbles: true,
      detail: {
        imageSrc: image.src,
        attachmentId: "",
      },
    });
  }
}
