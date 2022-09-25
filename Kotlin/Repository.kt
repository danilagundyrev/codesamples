package com.projectnametemplate.app.data

import androidx.lifecycle.LiveData
import kotlinx.coroutines.*

enum class DataStatus { LOADING, ERROR, READY }

/**
 * Concrete implementation to mediate data flows between local and remote sources.
 *
 * @property remoteDataSource remote server (backend).
 * @property localDataSource local DB (SQLite).
 * @property ioDispatcher dispatcher used for running coroutines.
 */
class Repository(
        private val remoteDataSource: IDataSource,
        private val localDataSource: IDataSource,
        private val ioDispatcher: CoroutineDispatcher = Dispatchers.IO
):
    IRepository {

    private var _myProfileId = -1


    /// PROFESSION ///

    override fun observeProfessions(): LiveData<Result<List<Profession>>> {
        return localDataSource.observeProfessions()
    }

    override suspend fun getProfessions(): Result<List<Profession>> {
        return localDataSource.getProfessions()
    }

    override suspend fun fetchProfessions() {
        val remoteProfessions = remoteDataSource.getProfessions()

        remoteProfessions.getOrElse {
            listOf()
        }.let {
            localDataSource.deleteProfessions()
            it.forEach { profession ->
                localDataSource.updateProfession(profession)
            }
        }
    }

    override suspend fun deleteProfessions() {
        withContext(ioDispatcher) {
            coroutineScope {
                launch { remoteDataSource.deleteProfessions() }
                launch { localDataSource.deleteProfessions() }
            }
        }
    }

    override fun observeProfession(id: Int): LiveData<Result<Profession>> {
        return localDataSource.observeProfession(id)
    }

    /**
     * Relies on [getProfessions] to fetch data and picks the profession with the same ID.
     */
    override suspend fun getProfession(id: Int): Result<Profession> {
        return localDataSource.getProfession(id)
    }

    override suspend fun fetchProfession(id: Int) {
        val remoteProfession = remoteDataSource.getProfession(id)

        if (remoteProfession.isSuccess)
            localDataSource.updateProfession(remoteProfession.getOrNull()!!)
    }

    override suspend fun updateProfession(profession: Profession) {
        coroutineScope {
            launch { remoteDataSource.updateProfession(profession) }
            launch { localDataSource.updateProfession(profession) }
        }
    }

    override suspend fun deleteProfession(id: Int) {
        coroutineScope {
            launch { remoteDataSource.deleteProfession(id) }
            launch { localDataSource.deleteProfession(id) }
        }
    }

    /// PROFILE ///

    override fun observeProfiles(): LiveData<Result<List<Profile>>> {
        return localDataSource.observeProfiles()
    }

    override suspend fun getProfiles(forceUpdate: Boolean): Result<List<Profile>> {
        if (forceUpdate) {
            try {
                updateProfilesFromRemoteDataSource()
            }
            catch (exception: Exception) {
                return Result.failure(exception)
            }
        }
        return localDataSource.getProfiles()
    }

    override suspend fun fetchProfiles() {
        updateProfilesFromRemoteDataSource()
    }

    override suspend fun deleteProfiles() {
        withContext(ioDispatcher) {
            coroutineScope {
                launch { remoteDataSource.deleteProfiles() }
                launch { localDataSource.deleteProfiles() }
            }
        }
    }

    override fun observeProfile(id: Int): LiveData<Result<Profile>> 
        return localDataSource.observeProfile(id)
    }

    /**
     * Relies on [getProfiles] to fetch data and picks the profile with the same ID.
     */
    override suspend fun getProfile(id: Int, forceUpdate: Boolean): Result<Profile> {
        if (forceUpdate) {
            updateProfileFromRemoteDataSource(id)
        }
        return localDataSource.getProfile(id)
    }

    override suspend fun getMyProfileId(userToken: String): Int {
        _myProfileId = updateMyProfileFromRemoteDataSource(userToken)
        return _myProfileId
    }

    override suspend fun fetchProfile(id: Int) {
        updateProfileFromRemoteDataSource(id)
    }

    override suspend fun updateMyProfile(userToken: String, profile: Profile) {
        coroutineScope {
            launch { remoteDataSource.updateProfile(profile) }
            launch { localDataSource.updateProfile(profile) }
        }
    }

    override suspend fun deleteMyProfile(userToken: String) {
        coroutineScope {
            launch { remoteDataSource.deleteProfile(_myProfileId) }
            launch { localDataSource.deleteProfile(_myProfileId) }
        }
    }

    private suspend fun updateProfilesFromRemoteDataSource() {
        val remoteProfiles = remoteDataSource.getProfiles()

        remoteProfiles.getOrElse {
            listOf()
        }.let {
            localDataSource.deleteProfiles()
            it.forEach { profile ->
                localDataSource.updateProfile(profile)
            }
        }
    }

    private suspend fun updateProfileFromRemoteDataSource(id: Int) {
        val remoteProfile = remoteDataSource.getProfile(id)

        if (remoteProfile.isSuccess)
            localDataSource.updateProfile(remoteProfile.getOrNull()!!)
    }

    private suspend fun updateMyProfileFromRemoteDataSource(userToken: String): Int {
        val remoteProfile = remoteDataSource.getMyProfile(userToken)

        remoteProfile.getOrElse {
            return ProfileViewModel.INVALID_PROFILE_ID
        }.let {
            localDataSource.updateProfile(it)
            return it.userId
        }
    }

    /// COMMON ///

    override suspend fun deleteAllData() {
        coroutineScope {
            launch { remoteDataSource.deleteAllData() }
            launch { localDataSource.deleteAllData() }
        }
    }
}
