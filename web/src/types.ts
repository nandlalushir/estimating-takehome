export type EstimateStatus = "Draft" | "Submitted" | "Approved" | "Rejected";

export interface Project {
  id: string;
  name: string;
}

export interface EstimateLine {
  id: string;
  catalogueItemId: string;
  code: string;
  description: string;
  unitOfMeasure: string;
  quantity: number;
  markupPercentage: number;
  rate: number;
  labourCost: number | null;
  netAmount: number;
  markupAmount: number;
  totalAmount: number;
}

export interface Estimate {
  id: string;
  projectId: string;
  description: string;
  pricingDate: string;
  status: EstimateStatus;
  total: number;
  lines: EstimateLine[];
}
