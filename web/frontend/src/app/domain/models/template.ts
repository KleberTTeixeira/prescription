/** ECF.10.02 – standard prescription text of the professional (used as a base and editable). */
export interface PrescriptionTemplate {
  id: number;
  title: string;
  content: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface TemplateRequest {
  title: string;
  content: string;
}
