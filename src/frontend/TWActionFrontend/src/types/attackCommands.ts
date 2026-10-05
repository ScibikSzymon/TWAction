export interface AttackCommandsSummary {
  totalCount: number;
  firstMinDepartureTime: string;
  lastMinDepartureTime: string;
  countByType: Record<string, number>;
  generatedAt: string;
}

export interface SendToPlemionaRozpiskiResponse {
  commandsSentCount: number;
  sentAt: string;
}
