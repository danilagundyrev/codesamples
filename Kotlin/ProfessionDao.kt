package com.projectnametemplate.app.data.local

import androidx.room.*
import com.projectnametemplate.app.data.Profession
import kotlinx.coroutines.flow.Flow

@Dao
interface ProfessionDao {
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insert(profession: Profession)

    @Update
    suspend fun update(profession: Profession)

    @Query("DELETE FROM profession WHERE id = :id")
    suspend fun delete(id: Int)

    @Query("DELETE FROM profession")
    suspend fun deleteAll()

    @Query("SELECT * from profession WHERE id = :id")
    fun observe(id: Int): Flow<Profession>

    @Query("SELECT * from profession ORDER BY name ASC")
    fun observeAll(): Flow<List<Profession>>

    @Query("SELECT * from profession WHERE id = :id")
    fun get(id: Int): Profession?

    @Query("SELECT * from profession ORDER BY name ASC")
    fun getAll(): List<Profession>
}
