import { Controller } from "framework";
import { AttachmentClickedEvent } from "../consts/events.js";

/** @typedef {import("../types/events").AttachmentClickedEvent} AttachmentClickedEvent */

/**
 * Controller for handling attachment modal interactions.
 *
 * @class AttachmentModalController
 * @extends Controller
 * @property {HTMLElement} element - The HTML element this controller is attached to.
 * @property {HTMLImageElement} attachmentImage - The image element where the attachment will be
 */
export default class AttachmentModalController extends Controller {
  connect() {
    this.subscribe(
      AttachmentClickedEvent.type,
      this._handleAttachmentClick.bind(this)
    );
  }

  /**
   * Handles a click of an attachment. Displays the modal and sets the image source.
   *
   * @private
   * @param {AttachmentClickedEvent} event
   * @returns {void}
   */
  _handleAttachmentClick(event) {
    const { imageSrc, attachmentId } = event.detail;
    this.attachmentImage.setAttribute("src", imageSrc);
    this.context.attachmentId = attachmentId;
  }
}
