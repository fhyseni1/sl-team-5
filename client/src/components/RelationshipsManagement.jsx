"use client";
import React, { useState, useEffect } from 'react';
import {
  Users,
  UserPlus,
  X,
  Edit,
  Trash2,
  Search,
  Shield,
  Check,
  Loader2,
  AlertCircle,
  UserCheck,
  Heart
} from 'lucide-react';
import relationshipService from '../../services/relationshipService';
import { userService } from '../../services/userService';
import { toast } from 'react-toastify';

const RelationshipsManagement = ({ currentUser }) => {
  const [relationships, setRelationships] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showAddForm, setShowAddForm] = useState(false);
  const [showEditForm, setShowEditForm] = useState(false);
  const [editingRelationship, setEditingRelationship] = useState(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [searchResults, setSearchResults] = useState([]);
  const [searching, setSearching] = useState(false);
  const [selectedUser, setSelectedUser] = useState(null);

  const [formData, setFormData] = useState({
    relatedUserId: '',
    relationshipType: 7, // Caregiver default
    canManageMedications: false,
    canViewHealthData: true,
    canScheduleAppointments: false,
  });

  // Relationship type options
  const relationshipTypes = [
    { value: 1, label: 'Spouse' },
    { value: 2, label: 'Parent' },
    { value: 3, label: 'Child' },
    { value: 4, label: 'Sibling' },
    { value: 5, label: 'Grandparent' },
    { value: 6, label: 'Grandchild' },
    { value: 7, label: 'Caregiver' },
    { value: 8, label: 'Friend' },
    { value: 9, label: 'Other' },
  ];

  useEffect(() => {
    if (currentUser?.id) {
      loadRelationships();
    }
  }, [currentUser?.id]);

  const loadRelationships = async () => {
    try {
      setLoading(true);
      const data = await relationshipService.getRelationshipsByUserId(currentUser.id);
      setRelationships(data || []);
    } catch (error) {
      console.error('Error loading relationships:', error);
      toast.error('Failed to load relationships');
    } finally {
      setLoading(false);
    }
  };

  const handleSearchUsers = async (term) => {
    if (term.length < 2) {
      setSearchResults([]);
      return;
    }

    try {
      setSearching(true);
      const users = await relationshipService.searchUsers(term);
      // Filter out current user and already added relationships
      const existingRelatedUserIds = relationships.map(r => r.relatedUserId);
      const filtered = users.filter(
        user => user.id !== currentUser.id && !existingRelatedUserIds.includes(user.id)
      );
      setSearchResults(filtered);
    } catch (error) {
      console.error('Error searching users:', error);
      toast.error('Failed to search users');
    } finally {
      setSearching(false);
    }
  };

  const handleSelectUser = (user) => {
    setSelectedUser(user);
    setFormData(prev => ({ ...prev, relatedUserId: user.id }));
    setSearchTerm(`${user.firstName} ${user.lastName} (${user.email})`);
    setSearchResults([]);
  };

  const handleAddRelationship = async (e) => {
    e.preventDefault();
    
    if (!formData.relatedUserId) {
      toast.error('Please select a user');
      return;
    }

    try {
      const relationshipData = {
        userId: currentUser.id,
        relatedUserId: formData.relatedUserId,
        relationshipType: parseInt(formData.relationshipType),
        canManageMedications: formData.canManageMedications,
        canViewHealthData: formData.canViewHealthData,
        canScheduleAppointments: formData.canScheduleAppointments,
      };

      await relationshipService.createRelationship(relationshipData);
      toast.success('Relationship added successfully!');
      
      // Reset form
      setFormData({
        relatedUserId: '',
        relationshipType: 7,
        canManageMedications: false,
        canViewHealthData: true,
        canScheduleAppointments: false,
      });
      setSelectedUser(null);
      setSearchTerm('');
      setShowAddForm(false);
      
      // Reload relationships
      await loadRelationships();
    } catch (error) {
      console.error('Error adding relationship:', error);
      const errorMsg = error.response?.data?.message || 'Failed to add relationship';
      toast.error(errorMsg);
    }
  };

  const handleEditRelationship = (relationship) => {
    setEditingRelationship(relationship);
    setFormData({
      relatedUserId: relationship.relatedUserId,
      relationshipType: relationship.relationshipType,
      canManageMedications: relationship.canManageMedications,
      canViewHealthData: relationship.canViewHealthData,
      canScheduleAppointments: relationship.canScheduleAppointments,
    });
    setSelectedUser({
      id: relationship.relatedUserId,
      firstName: relationship.relatedUserName.split(' ')[0],
      lastName: relationship.relatedUserName.split(' ').slice(1).join(' '),
    });
    setShowEditForm(true);
  };

  const handleUpdateRelationship = async (e) => {
    e.preventDefault();

    try {
      const updateData = {
        relationshipType: parseInt(formData.relationshipType),
        canManageMedications: formData.canManageMedications,
        canViewHealthData: formData.canViewHealthData,
        canScheduleAppointments: formData.canScheduleAppointments,
        isActive: true,
      };

      await relationshipService.updateRelationship(editingRelationship.id, updateData);
      toast.success('Relationship updated successfully!');
      
      setShowEditForm(false);
      setEditingRelationship(null);
      await loadRelationships();
    } catch (error) {
      console.error('Error updating relationship:', error);
      toast.error('Failed to update relationship');
    }
  };

  const handleDeleteRelationship = async (relationshipId) => {
    if (!confirm('Are you sure you want to remove this relationship?')) {
      return;
    }

    try {
      await relationshipService.deleteRelationship(relationshipId);
      toast.success('Relationship removed successfully!');
      await loadRelationships();
    } catch (error) {
      console.error('Error deleting relationship:', error);
      toast.error('Failed to remove relationship');
    }
  };

  const getRelationshipTypeLabel = (type) => {
    const relationship = relationshipTypes.find(r => r.value === type);
    return relationship ? relationship.label : 'Unknown';
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center p-8">
        <Loader2 className="w-8 h-8 animate-spin text-blue-400" />
      </div>
    );
  }

  return (
    <div className="bg-slate-800/50 backdrop-blur-xl rounded-2xl p-6 border border-slate-700/50">
      <div className="flex items-center justify-between mb-6">
        <div className="flex items-center gap-3">
          <div className="w-12 h-12 bg-gradient-to-br from-purple-500 to-pink-500 rounded-xl flex items-center justify-center">
            <Users className="w-6 h-6 text-white" />
          </div>
          <div>
            <h3 className="text-xl font-bold text-white">Family & Caregivers</h3>
            <p className="text-sm text-slate-400">
              Manage who can help you with your health
            </p>
          </div>
        </div>
        <button
          onClick={() => {
            setShowAddForm(true);
            setShowEditForm(false);
            setEditingRelationship(null);
            setFormData({
              relatedUserId: '',
              relationshipType: 7,
              canManageMedications: false,
              canViewHealthData: true,
              canScheduleAppointments: false,
            });
            setSelectedUser(null);
            setSearchTerm('');
          }}
          className="flex items-center gap-2 px-4 py-2 bg-gradient-to-r from-purple-600 to-pink-600 text-white rounded-xl hover:scale-105 transition-all font-medium"
        >
          <UserPlus className="w-5 h-5" />
          Add Relationship
        </button>
      </div>

      {/* Relationships List */}
      {relationships.length === 0 ? (
        <div className="text-center py-12">
          <Heart className="w-16 h-16 text-slate-600 mx-auto mb-4" />
          <p className="text-slate-400 mb-2">No relationships added yet</p>
          <p className="text-sm text-slate-500">
            Add family members or caregivers to help manage your health
          </p>
        </div>
      ) : (
        <div className="space-y-4">
          {relationships.map((relationship) => (
            <div
              key={relationship.id}
              className="bg-slate-700/50 rounded-xl p-4 border border-slate-600/50 hover:border-purple-500/50 transition-all"
            >
              <div className="flex items-start justify-between">
                <div className="flex-1">
                  <div className="flex items-center gap-3 mb-2">
                    <div className="w-10 h-10 bg-purple-500/20 rounded-lg flex items-center justify-center">
                      <UserCheck className="w-5 h-5 text-purple-400" />
                    </div>
                    <div>
                      <h4 className="text-white font-semibold">
                        {relationship.relatedUserName}
                      </h4>
                      <p className="text-sm text-slate-400">
                        {getRelationshipTypeLabel(relationship.relationshipType)}
                      </p>
                    </div>
                  </div>

                  {/* Permissions */}
                  <div className="flex flex-wrap gap-2 mt-3">
                    {relationship.canManageMedications && (
                      <span className="px-2 py-1 bg-emerald-500/20 text-emerald-400 text-xs rounded-full flex items-center gap-1">
                        <Shield className="w-3 h-3" />
                        Manage Medications
                      </span>
                    )}
                    {relationship.canViewHealthData && (
                      <span className="px-2 py-1 bg-blue-500/20 text-blue-400 text-xs rounded-full flex items-center gap-1">
                        <Shield className="w-3 h-3" />
                        View Health Data
                      </span>
                    )}
                    {relationship.canScheduleAppointments && (
                      <span className="px-2 py-1 bg-purple-500/20 text-purple-400 text-xs rounded-full flex items-center gap-1">
                        <Shield className="w-3 h-3" />
                        Schedule Appointments
                      </span>
                    )}
                    {!relationship.canManageMedications && 
                     !relationship.canViewHealthData && 
                     !relationship.canScheduleAppointments && (
                      <span className="px-2 py-1 bg-slate-600/50 text-slate-400 text-xs rounded-full">
                        Notifications Only
                      </span>
                    )}
                  </div>
                </div>

                <div className="flex items-center gap-2">
                  <button
                    onClick={() => handleEditRelationship(relationship)}
                    className="p-2 hover:bg-blue-500/20 text-blue-400 rounded-lg transition-all"
                    title="Edit"
                  >
                    <Edit className="w-4 h-4" />
                  </button>
                  <button
                    onClick={() => handleDeleteRelationship(relationship.id)}
                    className="p-2 hover:bg-red-500/20 text-red-400 rounded-lg transition-all"
                    title="Remove"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Add/Edit Form Modal */}
      {(showAddForm || showEditForm) && (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-50 flex items-center justify-center p-4">
          <div className="bg-slate-800/95 backdrop-blur-xl rounded-2xl max-w-md w-full border border-slate-700/50 shadow-2xl">
            <div className="flex items-center justify-between p-6 border-b border-slate-700/50">
              <h2 className="text-2xl font-bold text-white">
                {showEditForm ? 'Edit Relationship' : 'Add Family Member/Caregiver'}
              </h2>
              <button
                onClick={() => {
                  setShowAddForm(false);
                  setShowEditForm(false);
                  setEditingRelationship(null);
                  setSelectedUser(null);
                  setSearchTerm('');
                  setSearchResults([]);
                }}
                className="p-2 hover:bg-slate-700 rounded-lg transition-all"
              >
                <X className="w-5 h-5 text-slate-400" />
              </button>
            </div>

            <form
              onSubmit={showEditForm ? handleUpdateRelationship : handleAddRelationship}
              className="p-6 space-y-4"
            >
              {/* User Search (only for add) */}
              {showAddForm && (
                <div>
                  <label className="block text-sm font-medium text-slate-300 mb-2">
                    Search User by Email or Name
                  </label>
                  <div className="relative">
                    <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-slate-400" />
                    <input
                      type="text"
                      value={searchTerm}
                      onChange={(e) => {
                        setSearchTerm(e.target.value);
                        handleSearchUsers(e.target.value);
                      }}
                      placeholder="Type email or name..."
                      className="w-full pl-10 pr-4 py-3 bg-slate-700/80 border border-slate-600 rounded-xl text-white placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-purple-500"
                    />
                    {searching && (
                      <Loader2 className="absolute right-3 top-1/2 transform -translate-y-1/2 w-5 h-5 animate-spin text-purple-400" />
                    )}
                  </div>
                  
                  {/* Search Results */}
                  {searchResults.length > 0 && (
                    <div className="mt-2 max-h-48 overflow-y-auto bg-slate-700/80 rounded-xl border border-slate-600">
                      {searchResults.map((user) => (
                        <button
                          key={user.id}
                          type="button"
                          onClick={() => handleSelectUser(user)}
                          className="w-full px-4 py-3 text-left hover:bg-slate-600/50 transition-all border-b border-slate-600 last:border-b-0"
                        >
                          <div className="text-white font-medium">
                            {user.firstName} {user.lastName}
                          </div>
                          <div className="text-sm text-slate-400">{user.email}</div>
                        </button>
                      ))}
                    </div>
                  )}
                  
                  {selectedUser && (
                    <div className="mt-2 px-3 py-2 bg-purple-500/20 border border-purple-500/30 rounded-lg">
                      <div className="text-purple-300 text-sm font-medium">
                        Selected: {selectedUser.firstName} {selectedUser.lastName}
                      </div>
                    </div>
                  )}
                </div>
              )}

              {/* Relationship Type */}
              <div>
                <label className="block text-sm font-medium text-slate-300 mb-2">
                  Relationship Type
                </label>
                <select
                  value={formData.relationshipType}
                  onChange={(e) => setFormData({ ...formData, relationshipType: parseInt(e.target.value) })}
                  className="w-full px-4 py-3 bg-slate-700/80 border border-slate-600 rounded-xl text-white focus:outline-none focus:ring-2 focus:ring-purple-500"
                  required
                >
                  {relationshipTypes.map((type) => (
                    <option key={type.value} value={type.value}>
                      {type.label}
                    </option>
                  ))}
                </select>
              </div>

              {/* Permissions */}
              <div>
                <label className="block text-sm font-medium text-slate-300 mb-3">
                  Permissions
                </label>
                <div className="space-y-2">
                  <label className="flex items-center gap-3 p-3 bg-slate-700/50 rounded-lg hover:bg-slate-700/70 cursor-pointer transition-all">
                    <input
                      type="checkbox"
                      checked={formData.canViewHealthData}
                      onChange={(e) => setFormData({ ...formData, canViewHealthData: e.target.checked })}
                      className="w-5 h-5 text-purple-600 rounded focus:ring-purple-500"
                    />
                    <div>
                      <div className="text-white font-medium">View Health Data</div>
                      <div className="text-xs text-slate-400">Can view medications, appointments, and health metrics</div>
                    </div>
                  </label>

                  <label className="flex items-center gap-3 p-3 bg-slate-700/50 rounded-lg hover:bg-slate-700/70 cursor-pointer transition-all">
                    <input
                      type="checkbox"
                      checked={formData.canManageMedications}
                      onChange={(e) => setFormData({ ...formData, canManageMedications: e.target.checked })}
                      className="w-5 h-5 text-purple-600 rounded focus:ring-purple-500"
                    />
                    <div>
                      <div className="text-white font-medium">Manage Medications</div>
                      <div className="text-xs text-slate-400">Can add, edit, and delete medications</div>
                    </div>
                  </label>

                  <label className="flex items-center gap-3 p-3 bg-slate-700/50 rounded-lg hover:bg-slate-700/70 cursor-pointer transition-all">
                    <input
                      type="checkbox"
                      checked={formData.canScheduleAppointments}
                      onChange={(e) => setFormData({ ...formData, canScheduleAppointments: e.target.checked })}
                      className="w-5 h-5 text-purple-600 rounded focus:ring-purple-500"
                    />
                    <div>
                      <div className="text-white font-medium">Schedule Appointments</div>
                      <div className="text-xs text-slate-400">Can schedule appointments on your behalf</div>
                    </div>
                  </label>
                </div>
                <div className="mt-2 p-3 bg-blue-500/10 border border-blue-500/30 rounded-lg">
                  <div className="flex items-start gap-2">
                    <AlertCircle className="w-4 h-4 text-blue-400 mt-0.5" />
                    <div className="text-xs text-blue-300">
                      <strong>Note:</strong> All caregivers automatically receive notifications when you need help, regardless of permissions.
                    </div>
                  </div>
                </div>
              </div>

              {/* Form Actions */}
              <div className="flex items-center gap-3 pt-4"
                 style={{ top: '10%', paddingBottom: '400px' }} 
>
                <button
                  type="button"
                  onClick={() => {
                    setShowAddForm(false);
                    setShowEditForm(false);
                    setEditingRelationship(null);
                    setSelectedUser(null);
                    setSearchTerm('');
                  }}
                  className="flex-1 px-4 py-3 bg-slate-700 text-white rounded-xl hover:bg-slate-600 transition-all font-medium"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="flex-1 px-4 py-3 bg-gradient-to-r from-purple-600 to-pink-600 text-white rounded-xl hover:scale-105 transition-all font-medium"
                >
                  {showEditForm ? 'Update' : 'Add'} Relationship
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default RelationshipsManagement;

