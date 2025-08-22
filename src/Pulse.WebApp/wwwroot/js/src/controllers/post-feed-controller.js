import { Controller } from "framework";

export default class PostFeedController extends Controller {
  connect() {
    document.body.addEventListener("pulse:post-created", () => {
      this.isEmpty = false;
      this.emptyMessage.setAttribute("hidden", "");
    });
  }
}
