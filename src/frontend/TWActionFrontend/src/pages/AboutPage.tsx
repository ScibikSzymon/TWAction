import { useEffect, useState } from "react";
import { aboutService } from "../services/aboutService";
import type { SubscriptionPlanLimits } from "../types/subscriptionPlan";
import styles from "./AboutPage.module.css";

const displayLimit = (limit: number | null) =>
  limit === null ? "Bez limitu" : limit.toLocaleString("pl-PL");

const AboutPage = () => {
  const [plans, setPlans] = useState<SubscriptionPlanLimits[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const loadLimits = async () => {
      try {
        setPlans(await aboutService.getDefaultLimits());
      } catch (loadError) {
        console.error("Error loading subscription limits:", loadError);
        setError("Nie udało się pobrać informacji o limitach.");
      } finally {
        setIsLoading(false);
      }
    };

    void loadLimits();
  }, []);

  return (
    <div className={styles.container}>
      <header className={styles.header}>
        <span className={styles.eyebrow}>TWAction</span>
        <h1>Plany i domyślne limity</h1>
      </header>

      {isLoading && <div className={styles.message}>Ładowanie limitów...</div>}
      {error && <div className={styles.error}>{error}</div>}

      {!isLoading && !error && (
        <div className={styles.plans}>
          {plans.map((plan) => (
            <article
              className={`${styles.planCard} ${
                plan.subscriptionTier === "Premium" ? styles.premium : ""
              }`}
              key={plan.subscriptionTier}
            >
              <div className={styles.planHeading}>
                <div>
                  <span className={styles.planLabel}>Plan</span>
                  <h2>
                    {plan.subscriptionTier === "Premium"
                      ? "Premium"
                      : "Darmowy"}
                  </h2>
                </div>
                <span className={styles.planIcon}>
                  {plan.subscriptionTier === "Premium" ? "★" : "○"}
                </span>
              </div>

              <dl className={styles.limits}>
                <div>
                  <dt>Własne rozpiski</dt>
                  <dd>{displayLimit(plan.scheduleLimit)}</dd>
                </div>
                <div>
                  <dt>Własne szablony</dt>
                  <dd>{displayLimit(plan.templateLimit)}</dd>
                </div>
                <div>
                  <dt>Wgrania stanu wojsk</dt>
                  <dd>{displayLimit(plan.troopsUploadLimit)}</dd>
                </div>
                <div>
                  <dt>Okno limitu wgrań</dt>
                  <dd>{plan.troopsUploadWindowHours} h</dd>
                </div>
              </dl>

              <p className={styles.note}>
                Limit wgrań jest liczony osobno dla każdej rozpiski w ruchomym
                oknie czasowym.
              </p>
            </article>
          ))}
        </div>
      )}
    </div>
  );
};

export default AboutPage;
