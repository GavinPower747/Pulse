import { Controller } from "framework";

/**
 * ProfileController handles the user profile page interactions.
 * It manages the visibility of the user card and shows/hides the follow button in the header
 * based on whether the user card is in view using Intersection Observer.
 * @class ProfileController
 * @extends Controller
 * @property {HTMLElement} userCard - The user card element to observe.
 * @property {HTMLElement} headerFollowButton - The follow button in the header.
 * @property {boolean} userCardInView - Whether the user card is currently in view.
 */
export default class ProfileController extends Controller {
    intersectionObserver = null;

    connect() {
        // Initialize the intersection observer to watch the user card
        this._initializeIntersectionObserver();
    }

    disconnect() {
        if (this.intersectionObserver) {
            this.intersectionObserver.disconnect();
        }
        super.disconnect();
    }

    _initializeIntersectionObserver() {
        if (!this.userCard) {
            return;
        }

        this.intersectionObserver = new IntersectionObserver(
            ([entry]) => {
                this.userCardInView = entry.isIntersecting;
                this._updateHeaderFollowButtonVisibility();
            },
            { threshold: 0.1 }
        );

        this.intersectionObserver.observe(this.userCard.context);
    }

    _updateHeaderFollowButtonVisibility() {
        if (!this.headerFollowButton) {
            return;
        }

        if (this.userCardInView === true) {
            this.headerFollowButton.style.opacity = '0';
            this.headerFollowButton.style.transition = 'opacity 300ms ease-in';
            setTimeout(() => {
                if (this.userCardInView === true) {
                    this.headerFollowButton.setAttribute('hidden', '');
                }
            }, 300);
        } else {
            this.headerFollowButton.removeAttribute('hidden');
            this.headerFollowButton.style.opacity = '0';
            this.headerFollowButton.style.transition = 'opacity 300ms ease-out';
            // Force a reflow to ensure the display change takes effect
            this.headerFollowButton.offsetHeight;
            this.headerFollowButton.style.opacity = '1';
        }
    }
}
