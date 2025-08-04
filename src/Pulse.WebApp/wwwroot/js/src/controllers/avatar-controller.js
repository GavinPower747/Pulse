import { Controller } from "framework"

/**
 * AvatarController handles the avatar image loading and fallback to initials.
 * It listens for image load errors and displays initials if the image fails to load.
 * @class AvatarController
 * @extends Controller
 * @property {HTMLElement} element - The avatar element.
 * @property {HTMLElement} avatarInitials - The element displaying initials.
 * @property {HTMLElement} avatarImage - The element displaying the avatar image.
 * @property {boolean} imageFailed - Flag indicating if the image failed to load.
 */
export default class AvatarController extends Controller {
    connect() {
        if (this.imageFailed) {
            this.avatarInitials.removeAttribute("hidden");
            this.avatarImage?.setAttribute("hidden", "");
        } else {
            this.avatarImage?.addEventListener("error", () => {
                this.imageFailed = true;

                this.avatarInitials.removeAttribute("hidden");
                this.avatarImage?.setAttribute("hidden", "");
            });
        }
    }
}