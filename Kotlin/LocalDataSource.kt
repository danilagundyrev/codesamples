package com.projectnametemplate.app.data.local

import androidx.lifecycle.LiveData
import androidx.lifecycle.asLiveData
import androidx.lifecycle.map
import kotlinx.coroutines.CoroutineDispatcher
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext

/**
 * Concrete implementation of a data source working with the local db.
 */
class LocalDataSource internal constructor(
        private val professionDao: ProfessionDao,
        private val profileDao: ProfileDao,
        private val ioDispatcher: CoroutineDispatcher = Dispatchers.IO
): IDataSource {

    /// PROFESSION ///
    override fun observeProfessions(): LiveData<Result<List<Profession>>> {
        return professionDao.observeAll().asLiveData().map {
            Result.success(it)
        }
    }

    override suspend fun getProfessions(): Result<List<Profession>> = withContext(ioDispatcher) {
        return@withContext try {
            Result.success(professionDao.getAll())
        }
        catch (exception: Exception) {
            Result.failure(exception)
        }
    }

    override suspend fun fetchProfessions() {
        //NO-OP
    }

    override suspend fun deleteProfessions() {
        professionDao.deleteAll()
    }

    override fun observeProfession(id: Int): LiveData<Result<Profession>> {
        return professionDao.observe(id).asLiveData().map {
            Result.success(it)
        }
    }

    override suspend fun getProfession(id: Int): Result<Profession> = withContext(ioDispatcher) {
        try {
            val profession = professionDao.get(id)
            if (profession != null) {
                return@withContext Result.success(profession)
            }
            else {
                return@withContext Result.failure(Exception("Profession not found!"))
            }
        }
        catch (exception: Exception) {
            return@withContext Result.failure(exception)
        }
    }

    override suspend fun fetchProfession(id: Int) {
        //NO-OP
    }

    override suspend fun updateProfession(profession: Profession) {
        professionDao.insert(profession)
    }

    override suspend fun deleteProfession(id: Int) {
        professionDao.delete(id)
    }

    /// PROFILE ///
    override fun observeProfiles(): LiveData<Result<List<Profile>>> {
        return profileDao.observeAll().asLiveData().map {
            Result.success(it)
        }
    }

    override suspend fun getProfiles(): Result<List<Profile>> = withContext(ioDispatcher) {
        return@withContext try {
            Result.success(profileDao.getAll())
        }
        catch (exception: Exception) {
            Result.failure(exception)
        }
    }

    override suspend fun fetchProfiles() {
        //NO-OP
    }

    override suspend fun deleteProfiles() {
        profileDao.deleteAll()
    }

    override fun observeProfile(id: Int): LiveData<Result<Profile>> {
        return profileDao.observe(id).asLiveData().map {
            Result.success(it)
        }
    }

    override suspend fun getProfile(id: Int): Result<Profile> = withContext(ioDispatcher) {
        try {
            val profile = profileDao.get(id)
            if (profile != null) {
                return@withContext Result.success(profile)
            }
            else {
                return@withContext Result.failure(Exception("Profile not found!"))
            }
        }
        catch (exception: Exception) {
            return@withContext Result.failure(exception)
        }
    }

    override suspend fun getMyProfile(userToken: String): Result<Profile> =
        withContext(ioDispatcher) {
            return@withContext Result.failure(Exception("My Profile not found!"))
        }

    override suspend fun fetchProfile(id: Int) {
        //NO-OP
    }

    override suspend fun updateProfile(profile: Profile) {
        profileDao.insert(profile)
    }

    override suspend fun deleteProfile(id: Int) {
        profileDao.delete(id)
    }

   

    /// COMMON ///

    override suspend fun deleteAllData() {
        professionDao.deleteAll()
        profileDao.deleteAll()
    }
}
