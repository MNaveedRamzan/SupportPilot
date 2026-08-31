import jsPDF from "jspdf";
import type { Conversation } from "@/api/dashboard";

/**
 * Generates a simple text-based PDF of a conversation transcript and
 * triggers a browser download. Client-side generation is appropriate here
 * since this is an admin-convenience export, not a compliance/audit
 * document — a production system needing legal-record PDFs would generate
 * them server-side instead.
 */
export function exportTranscriptToPdf(conversation: Conversation): void {
  const doc = new jsPDF();
  const pageWidth = doc.internal.pageSize.getWidth();
  const margin = 15;
  const maxLineWidth = pageWidth - margin * 2;
  let y = 20;

  doc.setFontSize(16);
  doc.text("SupportPilot — Conversation Transcript", margin, y);
  y += 8;

  doc.setFontSize(10);
  doc.setTextColor(100);
  doc.text(
    `Conversation ID: ${conversation.id}`,
    margin,
    y
  );
  y += 5;
  doc.text(
    `Created: ${new Date(conversation.createdAt).toLocaleString()}`,
    margin,
    y
  );
  y += 5;
  doc.text(
    `Status: ${conversation.isEscalated ? "Escalated" : "Normal"}`,
    margin,
    y
  );
  y += 10;

  doc.setTextColor(0);

  for (const message of conversation.messages) {
    if (y > 270) {
      doc.addPage();
      y = 20;
    }

    doc.setFontSize(11);
    doc.setFont("helvetica", "bold");
    const roleLine =
      message.sentimentScore !== null
        ? `${message.role}  (sentiment: ${message.sentimentScore.toFixed(2)})`
        : message.role;
    doc.text(roleLine, margin, y);
    y += 6;

    doc.setFont("helvetica", "normal");
    doc.setFontSize(10);
    const contentLines = doc.splitTextToSize(message.content, maxLineWidth);
    doc.text(contentLines, margin, y);
    y += contentLines.length * 5 + 6;
  }

  doc.save(`conversation-${conversation.id.slice(0, 8)}.pdf`);
}