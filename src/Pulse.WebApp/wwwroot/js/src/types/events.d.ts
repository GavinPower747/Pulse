export type AttachmentClickedEvent = CustomEvent<AttachmentClickedDetail> & {
    type: "pulse:attachment-clicked";
};

export interface AttachmentClickedDetail {
    imageSrc: string;
    attachmentId: string;
}