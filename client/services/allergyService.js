const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5029/api";

export const allergyService = {
 addAllergy: async (allergyData) => {
  try {
    console.log("🟡 Sending allergy data to API:", allergyData);
    
    const response = await fetch(`${API_URL}/allergies`, {
      method: "POST",
      credentials: "include",
      headers: { 
        "Content-Type": "application/json",
      },
      body: JSON.stringify(allergyData),
    });
    
    console.log("🔵 Response status:", response.status);
    
    if (!response.ok) {
      
      const errorText = await response.text();
      console.log("🔴 Raw error response:", errorText);
      
      let errorMessage = `Server error: ${response.status}`;
      try {
        const errorData = JSON.parse(errorText);
        console.log("🔴 Parsed error data:", errorData);
        
        if (errorData.errors) {
          const validationErrors = Object.entries(errorData.errors)
            .map(([field, errors]) => `${field}: ${errors.join(', ')}`)
            .join('; ');
          errorMessage = `Validation errors: ${validationErrors}`;
        } else if (errorData.title) {
          errorMessage = errorData.title;
        }
      } catch (e) {
        errorMessage = errorText || `HTTP ${response.status}`;
      }
      
      throw new Error(errorMessage);
    }
    
    const result = await response.json();
    console.log("✅ Allergy added successfully:", result);
    return result;
    
  } catch (error) {
    console.error("❌ Error adding allergy:", error);
    throw error;
  }
},

  
  getUserAllergies: async (userId) => {
    try {
      const response = await fetch(`${API_URL}/allergies/user/${userId}`, {
        credentials: "include",
        headers: { "Content-Type": "application/json" },
      });
      
      if (response.ok) {
        return await response.json();
      }
      console.warn(`⚠️ Failed to get allergies for user ${userId}: ${response.status}`);
      return [];
    } catch (error) {
      console.error("Error fetching allergies:", error);
      return [];
    }
  },

  checkAllergyConflicts: async (userId, medicationName) => {
    try {
      const response = await fetch(`${API_URL}/allergies/check`, {
        method: "POST",
        credentials: "include",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          userId: userId,
          medicationName: medicationName
        }),
      });
      
      if (response.ok) {
        return await response.json();
      }
      return { hasConflicts: false, conflicts: [] };
    } catch (error) {
      console.error("Error checking allergy conflicts:", error);
      return { hasConflicts: false, conflicts: [] };
    }
  },

  deleteAllergy: async (allergyId) => {
    try {
     
      if (allergyId.startsWith('temp-')) {
        console.log("🗑️ Deleting temporary allergy locally");
        return true;
      }
      
      const response = await fetch(`${API_URL}/allergies/${allergyId}`, {
        method: "DELETE",
        credentials: "include",
      });
      
      return response.ok;
    } catch (error) {
      console.error("Error deleting allergy:", error);
      return false;
    }
  },

  getAllergyReport: async (userId) => {
    try {
      const response = await fetch(`${API_URL}/allergies/user/${userId}/report`, {
        credentials: "include",
        headers: { "Content-Type": "application/json" },
      });
      
      if (response.ok) {
        return await response.json();
      }
      return null;
    } catch (error) {
      console.error("Error fetching allergy report:", error);
      return null;
    }
  }
};