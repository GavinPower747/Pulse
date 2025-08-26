import { Controller } from "framework";
import { PostCreatedEvent } from "../consts/events.js";

export default class PostFeedController extends Controller {
  connect() {
    document.body.addEventListener(PostCreatedEvent.type, () => {
      this.isEmpty = false;
      this.emptyMessage.setAttribute("hidden", "");
    });
  }
}
