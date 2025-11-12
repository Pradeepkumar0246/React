export interface Document {
  documentId: number;
  employeeId: string;
  employeeName: string;
  category: 'IDProof' | 'Experience' | 'Skill' | 'Education' | 'Contract' | 'Offer' | 'Medical';
  fileName: string;
  filePath: string;
  uploadedDate: string;
  fileSize: number;
}

export interface DocumentFilters {
  search: string;
  employeeId: string;
  category: string;
}