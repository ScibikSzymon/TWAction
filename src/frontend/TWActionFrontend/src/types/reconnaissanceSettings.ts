export interface ReconnaissanceSettings {
  id: string;
  scheduleId: string;
  minDepartureTime: string;
  minArrivalTime: string;
  maxArrivalTime: string;
  minDistanceToFront: number;
  minSpyCount: number;
  maxPopulationInSourceVillage: number;
  skipNightSendings: boolean;
}

export interface SaveReconnaissanceSettingsRequest {
  minDepartureTime: string;
  minArrivalTime: string;
  maxArrivalTime: string;
  minDistanceToFront: number;
  minSpyCount: number;
  maxPopulationInSourceVillage: number;
  skipNightSendings: boolean;
}

export const getDefaultReconnaissanceSettings = (): Omit<
  SaveReconnaissanceSettingsRequest,
  "minDepartureTime" | "minArrivalTime" | "maxArrivalTime"
> & {
  minDepartureTime: Date;
  minArrivalTime: Date;
  maxArrivalTime: Date;
} => {
  const now = new Date();

  // Domyślna data to następny dzień 8 rano
  const minDepartureTime = new Date(now);
  minDepartureTime.setDate(minDepartureTime.getDate() + 1);
  minDepartureTime.setHours(8, 0, 0, 0);

  // Domyślny dzień to obecna data + 3 dni, 2:30
  const minArrivalTime = new Date(now);
  minArrivalTime.setDate(minArrivalTime.getDate() + 3);
  minArrivalTime.setHours(2, 30, 0, 0);

  // Domyślny dzień to obecna data + 3 dni, 4:30
  const maxArrivalTime = new Date(now);
  maxArrivalTime.setDate(maxArrivalTime.getDate() + 3);
  maxArrivalTime.setHours(4, 30, 0, 0);

  return {
    minDepartureTime,
    minArrivalTime,
    maxArrivalTime,
    minDistanceToFront: 10,
    minSpyCount: 50,
    maxPopulationInSourceVillage: 22000,
    skipNightSendings: true,
  };
};
