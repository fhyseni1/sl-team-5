export const fetchMedicationData = async (endpoint, options = {}) => {
  const response = await fetch(
    `${process.env.NEXT_PUBLIC_MEDICATION_API_URL}/${endpoint}`,
    {
      ...options,
      headers: {
        "Content-Type": "application/json",
        ...options.headers,
      },
    }
  );
  if (!response.ok)
    throw new Error(`Medication API request failed: ${response.statusText}`);
  return response.json();
};

export const fetchUserHealthData = async (endpoint, options = {}) => {
  const response = await fetch(
    `${process.env.NEXT_PUBLIC_USERHEALTH_API_URL}/${endpoint}`,
    {
      ...options,
      headers: {
        "Content-Type": "application/json",
        ...options.headers,
      },
    }
  );
  if (!response.ok)
    throw new Error(`UserHealth API request failed: ${response.statusText}`);
  return response.json();
};
