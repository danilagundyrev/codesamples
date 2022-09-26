package com.projectnametemplate.app.data.remote

import android.annotation.SuppressLint
import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.map
import com.projectnametemplate.app.data.*
import com.projectnametemplate.app.network.BackendApi

/**
 * Implementation of the data source working with the remote (backend) API.
 */
object RemoteDataSource: IDataSource {
    private val observableProfessions = MutableLiveData<Result<List<Profession>>>()
    private val observableProfiles = MutableLiveData<Result<List<Profile>>>()

    /// PROFESSION ///

    override fun observeProfessions(): LiveData<Result<List<Profession>>> {
        return observableProfessions
    }

    override suspend fun getProfessions(): Result<List<Profession>> {
        return try {
            Result.success(BackendApi.retrofitService.getProfessions())
        }
        catch (exception: Exception) {
            Result.failure(exception)
        }
    }

    @SuppressLint("NullSafeMutableLiveData")
    override suspend fun fetchProfessions() {
        observableProfessions.value = getProfessions()
    }

    override suspend fun deleteProfessions() {
        
    }

    override fun observeProfession(id: Int): LiveData<Result<Profession>> {
        return observableProfessions.map { professions ->
            if (professions.isSuccess) {
                val profession = professions.getOrNull()!!.firstOrNull { it.id == id }
                    ?: return@map Result.failure(Exception("Not found"))
                Result.success(profession)
            }
            else {
                Result.failure(professions.exceptionOrNull()!!)
            }
        }
    }

    override suspend fun getProfession(id: Int): Result<Profession> {
        return try {
            Result.success(BackendApi.retrofitService.getProfession(id))
        }
        catch (exception: Exception) {
            Result.failure(exception)
        }
    }

    override suspend fun fetchProfession(id: Int) { 
    }

    override suspend fun updateProfession(profession: Profession) {  
    }

    override suspend fun deleteProfession(id: Int) {
    }

    /// PROFILE ///

    override fun observeProfiles(): LiveData<Result<List<Profile>>> {
        return observableProfiles
    }

    override suspend fun getProfiles(): Result<List<Profile>> {
        return try {
            Result.success(BackendApi.retrofitService.getProfiles())
        }
        catch (exception: Exception) {
            Result.failure(exception)
        }
    }

    @SuppressLint("NullSafeMutableLiveData")
    override suspend fun fetchProfiles() {
        observableProfiles.value = getProfiles()
    }

    override suspend fun deleteProfiles() {
    }

    override fun observeProfile(id: Int): LiveData<Result<Profile>> {
        return observableProfiles.map { profiles ->
            if (profiles.isSuccess) {
                val profile = profiles.getOrNull()!!.firstOrNull { it.userId == id }
                    ?: return@map Result.failure(Exception("Not found"))
                Result.success(profile)
            }
            else {
                Result.failure(profiles.exceptionOrNull()!!)
            }
        }
    }

    override suspend fun getProfile(id: Int): Result<Profile> {
        return try {
            Result.success(BackendApi.retrofitService.getProfile(id))
        }
        catch (exception: Exception) {
            Result.failure(exception)
        }
    }

    override suspend fun getMyProfile(userToken: String): Result<Profile> {
        return try {
            Result.success(BackendApi.retrofitService.getMyProfile(userToken))
        }
        catch (exception: Exception) {
            Result.failure(exception)
        }
    }

    override suspend fun fetchProfile(id: Int) {
    }

    override suspend fun updateProfile(profile: Profile) {
    }

    override suspend fun deleteProfile(id: Int) {
    }
}
