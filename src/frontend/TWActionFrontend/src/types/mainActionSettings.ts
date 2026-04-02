export interface MainActionOffSettings {
  minOffUnits: number;
  minDistanceFromFront: number;
}

export interface MainActionCatasSettings {
  minCatasNumber: number;
  minDistanceFromFront: number;
  maxOffUnits: number;
}

export interface MainActionFakeOffSettings {
  minOffUnits: number;
  minDistanceFromFront: number;
}

export interface MainActionFakeDeffSettings {
  maxOffUnits: number;
  minDistanceFromFront: number;
}

export interface MainActionNobleSettings {
  minDistanceFromFront: number;
  minOffUnitsForOffNoble: number;
  minOffUnitsForFakeOffNoble: number;
  maxOffUnitsForDefNoble: number;
  minDeffUnitsForDefNoble: number;
}

export interface MainActionSettings {
  id: string;
  scheduleId: string;
  minDepartureTime: string;
  skipNightSendings: boolean;
  maxNobleDistance: number;
  offSettings: MainActionOffSettings;
  catasSettings: MainActionCatasSettings;
  fakeOffSettings: MainActionFakeOffSettings;
  fakeDeffSettings: MainActionFakeDeffSettings;
  nobleSettings: MainActionNobleSettings;
  playerNobleBudgets: Record<number, number>;
}

export interface SaveMainActionSettingsRequest {
  minDepartureTime: string;
  skipNightSendings: boolean;
  maxNobleDistance: number;
  offSettings: MainActionOffSettings;
  catasSettings: MainActionCatasSettings;
  fakeOffSettings: MainActionFakeOffSettings;
  fakeDeffSettings: MainActionFakeDeffSettings;
  nobleSettings: MainActionNobleSettings;
  playerNobleBudgets: Record<number, number>;
}

export const getDefaultMainActionSettings = (): Omit<
  SaveMainActionSettingsRequest,
  "minDepartureTime"
> & {
  minDepartureTime: Date;
} => {
  const now = new Date();

  // Domyślna data to następny dzień 8 rano
  const minDepartureTime = new Date(now);
  minDepartureTime.setDate(minDepartureTime.getDate() + 1);
  minDepartureTime.setHours(8, 0, 0, 0);

  return {
    minDepartureTime,
    skipNightSendings: true,
    maxNobleDistance: 49,
    offSettings: {
      minOffUnits: 18000,
      minDistanceFromFront: 5,
    },
    catasSettings: {
      minCatasNumber: 50,
      minDistanceFromFront: 5,
      maxOffUnits: 25000,
    },
    fakeOffSettings: {
      minOffUnits: 10000,
      minDistanceFromFront: 5,
    },
    fakeDeffSettings: {
      maxOffUnits: 10000,
      minDistanceFromFront: 5,
    },
    nobleSettings: {
      minDistanceFromFront: 5,
      minOffUnitsForOffNoble: 10000,
      minOffUnitsForFakeOffNoble: 7000,
      maxOffUnitsForDefNoble: 10000,
      minDeffUnitsForDefNoble: 10000,
    },
    playerNobleBudgets: {},
  };
};
