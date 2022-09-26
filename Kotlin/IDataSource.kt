package com.projectnametemplate.app.data

import androidx.lifecycle.LiveData

/**
 * Main entry point for accessing data.
 */
interface IDataSource {
    /// PROFESSION ///
    fun observeProfessions(): LiveData<Result<List<Profession>>>
    suspend fun getProfessions(): Result<List<Profession>>
    suspend fun fetchProfessions()
    suspend fun deleteProfessions()
    fun observeProfession(id: Int): LiveData<Result<Profession>>
    suspend fun getProfession(id: Int): Result<Profession>
    suspend fun fetchProfession(id: Int)
    suspend fun updateProfession(profession: Profession)
    suspend fun deleteProfession(id: Int)

    /// PROFILE ///
    fun observeProfiles(): LiveData<Result<List<Profile>>>
    suspend fun getProfiles(): Result<List<Profile>>
    suspend fun fetchProfiles()
    suspend fun deleteProfiles()
    fun observeProfile(id: Int): LiveData<Result<Profile>>
    suspend fun getProfile(id: Int): Result<Profile>
    suspend fun getMyProfile(userToken: String): Result<Profile>
    suspend fun fetchProfile(id: Int)
    suspend fun updateProfile(profile: Profile)
    suspend fun deleteProfile(id: Int)

    /// COMMON ///
    suspend fun deleteAllData()
}
