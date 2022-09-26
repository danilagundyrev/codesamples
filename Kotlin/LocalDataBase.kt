package com.projectnametemplate.app.data.local

import androidx.room.Database
import androidx.room.RoomDatabase
import androidx.room.TypeConverters
import com.projectnametemplate.app.data.*

@Database(entities = [Profession::class, Profile::class], version = 1, exportSchema = false)
@TypeConverters(Converters::class)
abstract class LocalDatabase : RoomDatabase(){
    abstract fun professionDao(): ProfessionDao
    abstract fun profileDao(): ProfileDao
}
