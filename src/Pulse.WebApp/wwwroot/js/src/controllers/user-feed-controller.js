import { Controller } from "framework";
import { PostCreatedEvent } from "../consts/events";

export default class UserFeedController extends Controller {
  connect() {
    if (!this.isEmpty) {
      this.emptyMessage.setAttribute("hidden", true);
    }

    this.subscribe(PostCreatedEvent.type, () =>
      this.emptyMessage.removeAttribute("hidden")
    );
  }
}
