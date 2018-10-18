-- MySQL Workbench Forward Engineering

SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;
SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;
SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='TRADITIONAL,ALLOW_INVALID_DATES';

-- -----------------------------------------------------
-- Schema lexis_nexis
-- -----------------------------------------------------

-- -----------------------------------------------------
-- Schema lexis_nexis
-- -----------------------------------------------------
CREATE SCHEMA IF NOT EXISTS `lexis_nexis` DEFAULT CHARACTER SET utf8 ;
USE `lexis_nexis` ;

-- -----------------------------------------------------
-- Table `lexis_nexis`.`APPL_LKUP`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `lexis_nexis`.`APPL_LKUP` ;

CREATE TABLE IF NOT EXISTS `lexis_nexis`.`APPL_LKUP` (
  `APPL_LKUP_REC_ID` INT(11) NOT NULL AUTO_INCREMENT,
  `APPL_LKUP_CD` INT(11) NOT NULL,
  `APPL_LKUP_CATG_NME` VARCHAR(255) NOT NULL,
  `APPL_LKUP_DESC` VARCHAR(255) NOT NULL,
  `APPL_LKUP_REC_EFF_DT` DATETIME NOT NULL,
  PRIMARY KEY (`APPL_LKUP_REC_ID`),
  INDEX `APPL_LKUP_CD_IDX` (`APPL_LKUP_CD` ASC))
ENGINE = InnoDB
AUTO_INCREMENT = 40
DEFAULT CHARACTER SET = utf8;


-- -----------------------------------------------------
-- Table `lexis_nexis`.`APPL_LOG`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `lexis_nexis`.`APPL_LOG` ;

CREATE TABLE IF NOT EXISTS `lexis_nexis`.`APPL_LOG` (
  `APPL_LOG_REC_ID` INT(11) NOT NULL AUTO_INCREMENT,
  `APPL_LOG_CD` INT(11) NOT NULL,
  `APPL_LOG_DESC` VARCHAR(5000) NOT NULL,
  `APPL_LOG_USR_NME` VARCHAR(255) NOT NULL,
  `APPL_LOG_DT` DATETIME NOT NULL,
  PRIMARY KEY (`APPL_LOG_REC_ID`),
  INDEX `APPL_LOG_CD_IDX` (`APPL_LOG_CD` ASC),
  INDEX `APPL_LOG_ID_IDX` (`APPL_LOG_REC_ID` ASC))
ENGINE = InnoDB
AUTO_INCREMENT = 4661
DEFAULT CHARACTER SET = utf8;


-- -----------------------------------------------------
-- Table `lexis_nexis`.`APPL_PARAM`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `lexis_nexis`.`APPL_PARAM` ;

CREATE TABLE IF NOT EXISTS `lexis_nexis`.`APPL_PARAM` (
  `APPL_PARAM_REC_ID` INT(11) NOT NULL AUTO_INCREMENT,
  `APPL_PARAM_NME` VARCHAR(255) NOT NULL,
  `APPL_PARAM_VAL` VARCHAR(255) NOT NULL,
  `APPL_PARAM_DESC` VARCHAR(5000) NULL DEFAULT NULL,
  `APPL_PARAM_EFF_DT` DATETIME NOT NULL,
  PRIMARY KEY (`APPL_PARAM_REC_ID`))
ENGINE = InnoDB
AUTO_INCREMENT = 26
DEFAULT CHARACTER SET = utf8;


-- -----------------------------------------------------
-- Table `lexis_nexis`.`APPL_RUN_STAT`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `lexis_nexis`.`APPL_RUN_STAT` ;

CREATE TABLE IF NOT EXISTS `lexis_nexis`.`APPL_RUN_STAT` (
  `JOB_TYP_CD` INT NOT NULL,
  `IS_RUNNING` TINYINT(1) NOT NULL DEFAULT 0,
  `MODIFIED_DT` DATETIME NOT NULL,
  PRIMARY KEY (`JOB_TYP_CD`),
  UNIQUE INDEX `JOB_TYP_ID_UNIQUE` (`JOB_TYP_CD` ASC),
  CONSTRAINT `JOB_TYP_CD_LKUP_CD_FKEY`
    FOREIGN KEY (`JOB_TYP_CD`)
    REFERENCES `lexis_nexis`.`APPL_LKUP` (`APPL_LKUP_CD`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB
AUTO_INCREMENT = 35409
DEFAULT CHARACTER SET = utf8;


-- -----------------------------------------------------
-- Table `lexis_nexis`.`APPL_SRCH_SRC`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `lexis_nexis`.`APPL_SRCH_SRC` ;

CREATE TABLE IF NOT EXISTS `lexis_nexis`.`APPL_SRCH_SRC` (
  `APPL_SRCH_SRC_REC_ID` INT(11) NOT NULL AUTO_INCREMENT,
  `APPL_SRCH_SRC_ID` INT(11) NOT NULL,
  `APPL_SRCH_SRC_NME` VARCHAR(5000) NOT NULL,
  `APPL_SRCH_SRC_FLDR` VARCHAR(5000) NOT NULL,
  `APPL_SRCH_SRC_REC_EFF_DT` DATETIME NOT NULL,
  `APPL_SRCH_SRC_REC_TRMN_DT` DATETIME NULL DEFAULT NULL,
  PRIMARY KEY (`APPL_SRCH_SRC_REC_ID`),
  INDEX `APPL_SRCH_SRC_ID_IDX` (`APPL_SRCH_SRC_REC_ID` ASC))
ENGINE = InnoDB
AUTO_INCREMENT = 19175501
DEFAULT CHARACTER SET = utf8;


-- -----------------------------------------------------
-- Table `lexis_nexis`.`APPL_SRCH_STAT`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `lexis_nexis`.`APPL_SRCH_STAT` ;

CREATE TABLE IF NOT EXISTS `lexis_nexis`.`APPL_SRCH_STAT` (
  `APPL_SRCH_STAT_REC_ID` INT(11) NOT NULL AUTO_INCREMENT,
  `APPL_SRCH_STAT_VAL` INT(11) NOT NULL,
  `APPL_SRCH_STAT_REC_EFF_DT` DATETIME NOT NULL,
  `APPL_SRCH_STAT_REC_LAST_UPD_DT` DATETIME NOT NULL,
  `APPL_SRCH_STAT_REC_TRMN_DT` DATETIME NULL DEFAULT NULL,
  PRIMARY KEY (`APPL_SRCH_STAT_REC_ID`),
  INDEX `APPL_SRCH_STAT_ID_IDX` (`APPL_SRCH_STAT_REC_ID` ASC))
ENGINE = InnoDB
AUTO_INCREMENT = 2639
DEFAULT CHARACTER SET = utf8;


-- -----------------------------------------------------
-- Table `lexis_nexis`.`APPL_USR_SRCH_STAT`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `lexis_nexis`.`APPL_USR_SRCH_STAT` ;

CREATE TABLE IF NOT EXISTS `lexis_nexis`.`APPL_USR_SRCH_STAT` (
  `APPL_USR_SRCH_STAT_REC_ID` INT(11) NOT NULL AUTO_INCREMENT,
  `APPL_USR_SRCH_SVD_NME` VARCHAR(5000) NOT NULL,
  `APPL_USR_SRCH_STAT_CD` INT(11) NOT NULL,
  `APPL_USR_SRCH_USR_NME` VARCHAR(255) NOT NULL,
  `APPL_USR_SRCH_RSLT_LOC` VARCHAR(1000) NULL DEFAULT NULL,
  `APPL_USR_SRCH_PCT_CMPLT` DECIMAL(5,2) NULL DEFAULT NULL,
  `APPL_USR_SRCH_QRY` TEXT NULL DEFAULT NULL,
  `APPL_USR_SRCH_SRC` TEXT NULL DEFAULT NULL,
  `APPL_USR_SRCH_MTHD` VARCHAR(255) NOT NULL,
  `APPL_USR_SRCH_ST_DT` DATETIME NULL DEFAULT NULL,
  `APPL_USR_SRCH_END_DT` DATETIME NULL DEFAULT NULL,
  `APPL_USR_SRCH_ST_INDX` INT(11) NULL DEFAULT NULL,
  `APPL_USR_SRCH_NUM_RSLTS` INT(11) NULL DEFAULT NULL,
  `APPL_USR_SRCH_REC_EFF_DT` DATETIME NOT NULL,
  `APPL_USR_SRCH_REC_TRMN_DT` DATETIME NULL DEFAULT NULL,
  `APPL_USR_SRCH_CURR_ST_DT` DATETIME NULL DEFAULT NULL,
  `APPL_USR_SRCH_CURR_END_DT` DATETIME NULL DEFAULT NULL,
  `APPL_USR_SRCH_ERR_MSG` VARCHAR(5000) NULL DEFAULT NULL,
  `APPL_USR_SRCH_RETRY_CNT` INT(11) NULL DEFAULT NULL,
  `APPL_USR_SRCH_CREATE_DT` DATETIME NULL DEFAULT NULL,
  `APPL_USR_SRCH_LN_ID` TEXT NULL,
  `APPL_USR_SRCH_RNG_RSLTS` INT(11) NULL,
  `APPL_USR_SRCH_QUEUE_POS` INT NULL,
  `APPL_USR_SRCH_FILE_SIZE` MEDIUMTEXT NULL,
  `APPL_USR_SRCH_FILE_SIZE_CHECK_DT` DATETIME NULL,
  `APPL_USR_SRCH_READY_TO_DWNLD` TINYINT(1) NULL,
  `ON_HOLD` TINYINT(1) NULL,
  `EMAILED` TINYINT(1) NULL,
  PRIMARY KEY (`APPL_USR_SRCH_STAT_REC_ID`),
  INDEX `APPL_USR_SRCH_STAT_ID_IDX` (`APPL_USR_SRCH_STAT_REC_ID` ASC),
  INDEX `APPL_USR_SRCH_STAT_CD_IDX` (`APPL_USR_SRCH_STAT_CD` ASC),
  INDEX `SRCH_SVD_NME_IDX` (`APPL_USR_SRCH_SVD_NME`(255) ASC),
  INDEX `SRCH_EFF_DT_IDX` (`APPL_USR_SRCH_REC_EFF_DT` ASC))
ENGINE = InnoDB
AUTO_INCREMENT = 165855
DEFAULT CHARACTER SET = utf8;


-- -----------------------------------------------------
-- Table `lexis_nexis`.`APPL_USR_SRCH_STAT_HIST`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `lexis_nexis`.`APPL_USR_SRCH_STAT_HIST` ;

CREATE TABLE IF NOT EXISTS `lexis_nexis`.`APPL_USR_SRCH_STAT_HIST` (
  `APPL_USR_SRCH_STAT_REC_ID` INT(11) NOT NULL AUTO_INCREMENT,
  `APPL_USR_SRCH_SVD_NME` VARCHAR(5000) NOT NULL,
  `APPL_USR_SRCH_STAT_CD` INT(11) NOT NULL,
  `APPL_USR_SRCH_USR_NME` VARCHAR(255) NOT NULL,
  `APPL_USR_SRCH_RSLT_LOC` VARCHAR(1000) NULL DEFAULT NULL,
  `APPL_USR_SRCH_PCT_CMPLT` DECIMAL(5,2) NULL DEFAULT NULL,
  `APPL_USR_SRCH_QRY` TEXT NULL DEFAULT NULL,
  `APPL_USR_SRCH_SRC` TEXT NULL DEFAULT NULL,
  `APPL_USR_SRCH_MTHD` VARCHAR(255) NOT NULL,
  `APPL_USR_SRCH_ST_DT` DATETIME NULL DEFAULT NULL,
  `APPL_USR_SRCH_END_DT` DATETIME NULL DEFAULT NULL,
  `APPL_USR_SRCH_ST_INDX` INT(11) NULL DEFAULT NULL,
  `APPL_USR_SRCH_NUM_RSLTS` INT(11) NULL DEFAULT NULL,
  `APPL_USR_SRCH_REC_EFF_DT` DATETIME NOT NULL,
  `APPL_USR_SRCH_REC_TRMN_DT` DATETIME NULL DEFAULT NULL,
  `APPL_USR_SRCH_CURR_ST_DT` DATETIME NULL DEFAULT NULL,
  `APPL_USR_SRCH_CURR_END_DT` DATETIME NULL DEFAULT NULL,
  `APPL_USR_SRCH_ERR_MSG` VARCHAR(5000) NULL DEFAULT NULL,
  `APPL_USR_SRCH_RETRY_CNT` INT(11) NULL DEFAULT NULL,
  `APPL_USR_SRCH_CREATE_DT` DATETIME NULL DEFAULT NULL,
  `APPL_USR_SRCH_LN_ID` TEXT NULL,
  `APPL_USR_SRCH_RNG_RSLTS` INT(11) NULL,
  `APPL_USR_SRCH_QUEUE_POS` INT NULL,
  `APPL_USR_SRCH_FILE_SIZE` MEDIUMTEXT NULL,
  `APPL_USR_SRCH_FILE_SIZE_CHECK_DT` DATETIME NULL,
  `APPL_USR_SRCH_READY_TO_DWNLD` TINYINT(1) NULL,
  `ON_HOLD` TINYINT(1) NULL,
  `EMAILED` TINYINT(1) NULL,
  PRIMARY KEY (`APPL_USR_SRCH_STAT_REC_ID`),
  INDEX `APPL_USR_SRCH_STAT_ID_IDX` (`APPL_USR_SRCH_STAT_REC_ID` ASC),
  INDEX `APPL_USR_SRCH_STAT_CD_IDX` (`APPL_USR_SRCH_STAT_CD` ASC),
  INDEX `SRCH_SVD_NME_IDX` (`APPL_USR_SRCH_SVD_NME`(255) ASC),
  INDEX `SRCH_EFF_DT_IDX` (`APPL_USR_SRCH_REC_EFF_DT` ASC))
ENGINE = InnoDB
AUTO_INCREMENT = 458279
DEFAULT CHARACTER SET = utf8;


-- -----------------------------------------------------
-- Table `lexis_nexis`.`APPL_RUN_STAT_HIST`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `lexis_nexis`.`APPL_RUN_STAT_HIST` ;

CREATE TABLE IF NOT EXISTS `lexis_nexis`.`APPL_RUN_STAT_HIST` (
  `REC_ID` INT NOT NULL AUTO_INCREMENT,
  `JOB_TYP_CD` INT NOT NULL,
  `IS_RUNNING` TINYINT(1) NOT NULL DEFAULT 0,
  `MODIFIED_DT` DATETIME NOT NULL,
  `TERMINATION_DT` DATETIME NOT NULL,
  PRIMARY KEY (`REC_ID`))
ENGINE = InnoDB
AUTO_INCREMENT = 35409
DEFAULT CHARACTER SET = utf8;

USE `lexis_nexis` ;

-- -----------------------------------------------------
-- procedure p_add_log
-- -----------------------------------------------------

USE `lexis_nexis`;
DROP procedure IF EXISTS `lexis_nexis`.`p_add_log`;

DELIMITER $$
USE `lexis_nexis`$$
CREATE PROCEDURE `p_add_log`(LOG_CD INT, LOG_MSG VARCHAR(5000), LOG_USR VARCHAR(255))
BEGIN
	INSERT INTO APPL_LOG (APPL_LOG_CD, APPL_LOG_DESC, APPL_LOG_USR_NME, APPL_LOG_DT)
    VALUES (LOG_CD, LOG_MSG, LOG_USR, NOW());
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure p_add_usr_srch
-- -----------------------------------------------------

USE `lexis_nexis`;
DROP procedure IF EXISTS `lexis_nexis`.`p_add_usr_srch`;

DELIMITER $$
USE `lexis_nexis`$$
CREATE PROCEDURE `p_add_usr_srch`(SRCH_NME VARCHAR(5000), SRCH_USR VARCHAR(255), SRCH_QRY TEXT,
								SRCH_SRC TEXT, SRCH_TO_DT DATETIME, SRCH_FROM_DT DATETIME, SRCH_MTHD VARCHAR(255))
BEGIN
	DECLARE STAT_CD INT DEFAULT 1;
	DECLARE POS INT DEFAULT 1;
    
	SELECT APPL_LKUP_CD INTO STAT_CD FROM APPL_LKUP WHERE APPL_LKUP_DESC = 'Pending';
    
    # Determine the position in the queue
    SELECT COALESCE(MAX(APPL_USR_SRCH_QUEUE_POS),0)+ 1 INTO POS FROM APPL_USR_SRCH_STAT
    WHERE APPL_USR_SRCH_REC_TRMN_DT IS NULL
	AND APPL_USR_SRCH_STAT_CD IN 
	(SELECT APPL_LKUP_CD FROM APPL_LKUP WHERE APPL_LKUP_DESC IN ('Pending','Processing'));
    
	INSERT INTO APPL_USR_SRCH_STAT (APPL_USR_SRCH_SVD_NME, APPL_USR_SRCH_USR_NME,
		APPL_USR_SRCH_STAT_CD, APPL_USR_SRCH_REC_EFF_DT, APPL_USR_SRCH_REC_TRMN_DT,
		APPL_USR_SRCH_RSLT_LOC, APPL_USR_SRCH_PCT_CMPLT, APPL_USR_SRCH_QRY, APPL_USR_SRCH_SRC, 
        APPL_USR_SRCH_ST_DT, APPL_USR_SRCH_END_DT,
        APPL_USR_SRCH_CURR_ST_DT, APPL_USR_SRCH_CURR_END_DT, APPL_USR_SRCH_ST_INDX, 
        APPL_USR_SRCH_MTHD, APPL_USR_SRCH_RETRY_CNT, APPL_USR_SRCH_CREATE_DT, APPL_USR_SRCH_QUEUE_POS)
	VALUES (SRCH_NME, SRCH_USR, STAT_CD, NOW(), NULL, NULL, 0.0, SRCH_QRY, SRCH_SRC, SRCH_FROM_DT, 
    SRCH_TO_DT, SRCH_FROM_DT, SRCH_TO_DT, 1, SRCH_MTHD,0, NOW(), POS);
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure p_del_usr_records
-- -----------------------------------------------------

USE `lexis_nexis`;
DROP procedure IF EXISTS `lexis_nexis`.`p_del_usr_records`;

DELIMITER $$
USE `lexis_nexis`$$
CREATE  PROCEDURE `p_del_usr_records`()
BEGIN
	DECLARE NO_MTHS INT DEFAULT 3;
    
    SELECT APPL_PARAM_VAL INTO NO_MTHS FROM APPL_PARAM WHERE APPL_PARAM_NME = 'MTHS_TO_DEL';
    
    CREATE TEMPORARY TABLE IF NOT EXISTS tmp_tbl AS (
	SELECT DISTINCT APPL_USR_SRCH_RSLT_LOC FROM APPL_USR_SRCH_STAT
    WHERE ADDDATE(APPL_USR_SRCH_REC_EFF_DT,INTERVAL NO_MTHS MONTH) < NOW()
    AND APPL_USR_SRCH_RSLT_LOC IS NOT NULL);
    
    UPDATE APPL_USR_SRCH_STAT SET APPL_USR_SRCH_USR_NME = 'DELETED', APPL_USR_SRCH_REC_TRMN_DT = NOW() WHERE ADDDATE(APPL_USR_SRCH_REC_EFF_DT,INTERVAL NO_MTHS MONTH) < NOW();
    UPDATE APPL_LOG SET APPL_LOG_USR_NME = 'DELETED' WHERE ADDDATE(APPL_LOG_DT, INTERVAL NO_MTHS MONTH) < NOW();
    
    SELECT APPL_USR_SRCH_RSLT_LOC AS RSLT_LOC FROM tmp_tbl;
    
	DROP TEMPORARY TABLE IF EXISTS tmp_tbl;

END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure p_del_usr_srch
-- -----------------------------------------------------

USE `lexis_nexis`;
DROP procedure IF EXISTS `lexis_nexis`.`p_del_usr_srch`;

DELIMITER $$
USE `lexis_nexis`$$
CREATE PROCEDURE `p_del_usr_srch`(SRCH_NME VARCHAR(5000))
BEGIN  
	DECLARE POS INT;
    
    SELECT APPL_USR_SRCH_QUEUE_POS INTO POS FROM APPL_USR_SRCH_STAT
    WHERE APPL_USR_SRCH_SVD_NME = SRCH_NME AND APPL_USR_SRCH_REC_TRMN_DT IS NULL;
    
	# perform update first so trigger will move record to history table
	UPDATE APPL_USR_SRCH_STAT SET APPL_USR_SRCH_REC_TRMN_DT = NOW()
    WHERE APPL_USR_SRCH_SVD_NME = SRCH_NME AND APPL_USR_SRCH_REC_TRMN_DT IS NULL;
    
    DELETE FROM APPL_USR_SRCH_STAT WHERE APPL_USR_SRCH_SVD_NME = SRCH_NME AND APPL_USR_SRCH_REC_TRMN_DT IS NOT NULL;
    
    # update the queue positions
    UPDATE APPL_USR_SRCH_STAT
    SET APPL_USR_SRCH_QUEUE_POS = APPL_USR_SRCH_QUEUE_POS - 1
    WHERE APPL_USR_SRCH_QUEUE_POS > POS 
    AND APPL_USR_SRCH_REC_TRMN_DT IS NULL
	AND APPL_USR_SRCH_STAT_CD IN 
	(SELECT APPL_LKUP_CD FROM APPL_LKUP WHERE APPL_LKUP_DESC IN ('Pending','Processing'));
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure p_get_appl_lkup
-- -----------------------------------------------------

USE `lexis_nexis`;
DROP procedure IF EXISTS `lexis_nexis`.`p_get_appl_lkup`;

DELIMITER $$
USE `lexis_nexis`$$
CREATE PROCEDURE `p_get_appl_lkup`()
BEGIN	   
	SELECT APPL_LKUP_CD, APPL_LKUP_CATG_NME, APPL_LKUP_DESC
	FROM APPL_LKUP;
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure p_get_appl_param
-- -----------------------------------------------------

USE `lexis_nexis`;
DROP procedure IF EXISTS `lexis_nexis`.`p_get_appl_param`;

DELIMITER $$
USE `lexis_nexis`$$
CREATE  PROCEDURE `p_get_appl_param`()
BEGIN	   
	SELECT APPL_PARAM_NME, APPL_PARAM_VAL, APPL_PARAM_DESC
	FROM APPL_PARAM;
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure p_get_appl_srch_stat
-- -----------------------------------------------------

USE `lexis_nexis`;
DROP procedure IF EXISTS `lexis_nexis`.`p_get_appl_srch_stat`;

DELIMITER $$
USE `lexis_nexis`$$
CREATE PROCEDURE `p_get_appl_srch_stat`(OUT REMAINING INT, OUT CURRENT INT)
BEGIN	  
	DECLARE EFF_DT DATETIME;
	DECLARE CNT INT;
	DECLARE MAX_CNT INT;

	SELECT APPL_PARAM_VAL INTO MAX_CNT FROM APPL_PARAM WHERE APPL_PARAM_NME = 'SRCH_PR_HR';
 
	SELECT APPL_SRCH_STAT_REC_EFF_DT,APPL_SRCH_STAT_VAL INTO EFF_DT, CNT
    FROM APPL_SRCH_STAT WHERE APPL_SRCH_STAT_REC_TRMN_DT IS NULL;
    
    IF HOUR(EFF_DT) != HOUR(NOW()) THEN SET REMAINING  = MAX_CNT; SET CURRENT = 0;
    ELSE SET REMAINING = MAX_CNT - CNT; SET CURRENT = CNT;
    END IF;
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure p_get_nex_run_window
-- -----------------------------------------------------

USE `lexis_nexis`;
DROP procedure IF EXISTS `lexis_nexis`.`p_get_nex_run_window`;

DELIMITER $$
USE `lexis_nexis`$$
CREATE PROCEDURE `p_get_nex_run_window`(OUT RUN_WINDOW DATETIME)
BEGIN
	DECLARE WK_DY INT;
    DECLARE RUN_HR VARCHAR(255);
    
    SELECT WEEKDAY(NOW()) INTO WK_DY;
    
    IF WK_DY IN (6,7) 
    THEN
		SELECT APPL_PARAM_VAL INTO RUN_HR FROM APPL_PARAM WHERE APPL_PARAM_NME = 'WEEKEND_START';
	ELSE
		SELECT APPL_PARAM_VAL INTO RUN_HR FROM APPL_PARAM WHERE APPL_PARAM_NME = 'WEEKDAY_START';
	END IF;
    
    SELECT  STR_TO_DATE(concat(MONTH(NOW()),'/',DAY(NOW()),'/',YEAR(NOW()),' ',RUN_HR), '%m/%d/%Y %H:%i') INTO RUN_WINDOW;

END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure p_get_run_stat
-- -----------------------------------------------------

USE `lexis_nexis`;
DROP procedure IF EXISTS `lexis_nexis`.`p_get_run_stat`;

DELIMITER $$
USE `lexis_nexis`$$
CREATE PROCEDURE `p_get_run_stat`(IN JOB_TYP INT, OUT RUN_STAT BOOL)
BEGIN	  
	SELECT IS_RUNNING INTO RUN_STAT
    FROM APPL_RUN_STAT WHERE JOB_TYP_CD = JOB_TYP ORDER BY MODIFIED_DT DESC LIMIT 1;
    
    IF RUN_STAT IS NULL THEN
		SET RUN_STAT = FALSE;
    END IF;
    
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure p_get_srch_queue
-- -----------------------------------------------------

USE `lexis_nexis`;
DROP procedure IF EXISTS `lexis_nexis`.`p_get_srch_queue`;

DELIMITER $$
USE `lexis_nexis`$$
CREATE PROCEDURE `p_get_srch_queue`()
BEGIN
	DECLARE CNT INT;
	# rotate queue order
	SELECT COUNT(*) INTO CNT FROM APPL_USR_SRCH_STAT S
		  INNER JOIN APPL_LKUP L ON S.APPL_USR_SRCH_STAT_CD = L.APPL_LKUP_CD
		  WHERE S.APPL_USR_SRCH_REC_TRMN_DT IS NULL
		  AND L.APPL_LKUP_DESC IN ('Pending','Processing')
          AND NOT COALESCE(S.ON_HOLD, FALSE);
          
	UPDATE APPL_USR_SRCH_STAT SET APPL_USR_SRCH_QUEUE_POS = 
			(CASE WHEN APPL_USR_SRCH_QUEUE_POS <> CNT THEN APPL_USR_SRCH_QUEUE_POS ELSE 0 END) + 1
    WHERE APPL_USR_SRCH_REC_TRMN_DT IS NULL
		AND APPL_USR_SRCH_STAT_CD IN 
			(SELECT APPL_LKUP_CD FROM APPL_LKUP WHERE APPL_LKUP_DESC IN ('Pending','Processing'))
            AND APPL_USR_SRCH_QUEUE_POS != 
			(CASE WHEN APPL_USR_SRCH_QUEUE_POS <> CNT THEN APPL_USR_SRCH_QUEUE_POS ELSE 0 END) + 1
            AND NOT COALESCE(ON_HOLD, FALSE);


  SELECT S.APPL_USR_SRCH_STAT_REC_ID, S.APPL_USR_SRCH_SVD_NME, 
		S.APPL_USR_SRCH_STAT_CD, S.APPL_USR_SRCH_RSLT_LOC,	 S.APPL_USR_SRCH_QRY, 
        S.APPL_USR_SRCH_SRC, S.APPL_USR_SRCH_ST_DT, S.APPL_USR_SRCH_END_DT, 
        S.APPL_USR_SRCH_PCT_CMPLT, S.APPL_USR_SRCH_USR_NME, S.APPL_USR_SRCH_ST_INDX, 
        S.APPL_USR_SRCH_NUM_RSLTS, S.APPL_USR_SRCH_MTHD, S.APPL_USR_SRCH_CURR_ST_DT, 
        S.APPL_USR_SRCH_CURR_END_DT, S.APPL_USR_SRCH_RETRY_CNT, S.APPL_USR_SRCH_LN_ID, S.APPL_USR_SRCH_RNG_RSLTS,
        S.APPL_USR_SRCH_QUEUE_POS, S.EMAILED
  FROM APPL_USR_SRCH_STAT S
  INNER JOIN APPL_LKUP L ON S.APPL_USR_SRCH_STAT_CD = L.APPL_LKUP_CD
  WHERE S.APPL_USR_SRCH_REC_TRMN_DT IS NULL
  AND L.APPL_LKUP_DESC IN ('Pending','Processing')
  AND NOT COALESCE(S.ON_HOLD, FALSE)
  ORDER BY S.APPL_USR_SRCH_QUEUE_POS ASC;
 END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure p_get_srch_src
-- -----------------------------------------------------

USE `lexis_nexis`;
DROP procedure IF EXISTS `lexis_nexis`.`p_get_srch_src`;

DELIMITER $$
USE `lexis_nexis`$$
CREATE PROCEDURE `p_get_srch_src`()
BEGIN	   
	SELECT APPL_SRCH_SRC_ID, APPL_SRCH_SRC_NME, APPL_SRCH_SRC_FLDR
	FROM APPL_SRCH_SRC WHERE APPL_SRCH_SRC_REC_TRMN_DT IS NULL;
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure p_get_usr_srch
-- -----------------------------------------------------

USE `lexis_nexis`;
DROP procedure IF EXISTS `lexis_nexis`.`p_get_usr_srch`;

DELIMITER $$
USE `lexis_nexis`$$
CREATE PROCEDURE `p_get_usr_srch`(USR_NME VARCHAR(255))
BEGIN
 SELECT S.APPL_USR_SRCH_STAT_REC_ID, S.APPL_USR_SRCH_SVD_NME, 
 DATE_FORMAT(S.APPL_USR_SRCH_CREATE_DT, '%b %d %Y %h:%i %p') AS APPL_USR_SRCH_REC_EFF_DT, 
	CASE WHEN L.APPL_LKUP_DESC = 'INVALID' THEN CONCAT('Invalid Search! ',S.APPL_USR_SRCH_ERR_MSG) 
		WHEN L.APPL_LKUP_DESC = 'Complete' AND S.APPL_USR_SRCH_NUM_RSLTS = 0 THEN 'No Results!'
		ELSE L.APPL_LKUP_DESC END AS APPL_LKUP_DESC,     
    S.APPL_USR_SRCH_RSLT_LOC,
    CASE WHEN L.APPL_LKUP_DESC = 'Complete' AND S.APPL_USR_SRCH_NUM_RSLTS = 0 THEN 'Cancel' 
		WHEN L.APPL_LKUP_DESC = 'Complete' AND S.APPL_USR_SRCH_READY_TO_DWNLD = TRUE THEN 'Download'
        WHEN L.APPL_LKUP_DESC = 'Complete' AND S.APPL_USR_SRCH_READY_TO_DWNLD = FALSE THEN ''
		WHEN L.APPL_LKUP_DESC = 'Invalid'  THEN 'Cancel'
        WHEN L.APPL_LKUP_DESC = 'Pending'  THEN 'Cancel'
        ELSE ''
	END AS APPL_USR_SRCH_ACTN,
	CONVERT(TRUNCATE((S.APPL_USR_SRCH_PCT_CMPLT * 100),0),CHAR) as PCT_CMPLT,
	S.APPL_USR_SRCH_NUM_RSLTS,
	CASE WHEN L.APPL_LKUP_DESC = 'Complete' OR L.APPL_LKUP_DESC = 'Invalid' THEN 'N/A' 
		WHEN S.ON_HOLD = TRUE THEN 'On Hold'
		ELSE S.APPL_USR_SRCH_QUEUE_POS END AS QUEUE_POS,
	S.APPL_USR_SRCH_QRY
  FROM APPL_USR_SRCH_STAT S
  INNER JOIN APPL_LKUP L ON (S.APPL_USR_SRCH_STAT_CD = L.APPL_LKUP_CD AND L.APPL_LKUP_CATG_NME = 'SRCH_STAT')
  WHERE S.APPL_USR_SRCH_REC_TRMN_DT IS NULL
  AND S.APPL_USR_SRCH_USR_NME = USR_NME
  ORDER BY S.APPL_USR_SRCH_CREATE_DT DESC;
 END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure p_incmnt_appl_srch_stat
-- -----------------------------------------------------

USE `lexis_nexis`;
DROP procedure IF EXISTS `lexis_nexis`.`p_incmnt_appl_srch_stat`;

DELIMITER $$
USE `lexis_nexis`$$
CREATE PROCEDURE `p_incmnt_appl_srch_stat`()
BEGIN
	DECLARE EFF_DT DATETIME;
	DECLARE CNT INT;
    
	START TRANSACTION;
        
		SELECT APPL_SRCH_STAT_REC_EFF_DT,APPL_SRCH_STAT_VAL INTO EFF_DT, CNT
		FROM APPL_SRCH_STAT WHERE APPL_SRCH_STAT_REC_TRMN_DT IS NULL;
        
		IF HOUR(EFF_DT) != HOUR(NOW())
		THEN
			UPDATE APPL_SRCH_STAT SET APPL_SRCH_STAT_REC_TRMN_DT = NOW()
			WHERE APPL_SRCH_STAT_REC_TRMN_DT IS NULL;
            
            INSERT INTO APPL_SRCH_STAT (APPL_SRCH_STAT_VAL, APPL_SRCH_STAT_REC_EFF_DT,  APPL_SRCH_STAT_REC_LAST_UPD_DT, APPL_SRCH_STAT_REC_TRMN_DT)
            VALUES (1, NOW(), NOW(), NULL);
		ELSE	
			UPDATE APPL_SRCH_STAT SET APPL_SRCH_STAT_REC_LAST_UPD_DT = NOW(), APPL_SRCH_STAT_VAL = CNT + 1
			WHERE APPL_SRCH_STAT_REC_TRMN_DT IS NULL;
		END IF;
    
    COMMIT;
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure p_set_run_stat
-- -----------------------------------------------------

USE `lexis_nexis`;
DROP procedure IF EXISTS `lexis_nexis`.`p_set_run_stat`;

DELIMITER $$
USE `lexis_nexis`$$
CREATE PROCEDURE `p_set_run_stat`(JOB_TYP INT, RUNNING BOOL)
BEGIN  
	START TRANSACTION;
		
        INSERT INTO APPL_RUN_STAT (JOB_TYP_CD, IS_RUNNING, MODIFIED_DT) 
        VALUES (JOB_TYP, RUNNING, NOW())
        ON DUPLICATE KEY UPDATE IS_RUNNING = RUNNING, MODIFIED_DT = NOW();
    
    COMMIT;
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure p_upd_usr_srch
-- -----------------------------------------------------

USE `lexis_nexis`;
DROP procedure IF EXISTS `lexis_nexis`.`p_upd_usr_srch`;

DELIMITER $$
USE `lexis_nexis`$$
CREATE PROCEDURE `p_upd_usr_srch`(SRCH_NME VARCHAR(5000), STAT_CD INT, RSLT_LOC VARCHAR(1000), 
								PCT_CMPLT DECIMAL(5,2), ST_INDX INT, NUM_RSLTS INT,
								CURR_ST DATETIME, CURR_END DATETIME, ERR_MSG VARCHAR(5000),RETRY_CNT INT, 
                                LN_ID TEXT, RNG_RSLTS INT, FILE_SIZE LONG, FILE_CHECK DATETIME, READY_FLAG BOOL, QUEUE_POS INT, EMAILED BOOL)
BEGIN		
	UPDATE APPL_USR_SRCH_STAT 
	SET APPL_USR_SRCH_SVD_NME = SRCH_NME, 
	APPL_USR_SRCH_STAT_CD = STAT_CD, 
	APPL_USR_SRCH_REC_EFF_DT = NOW(), 
	APPL_USR_SRCH_REC_TRMN_DT = NULL, 
	APPL_USR_SRCH_RSLT_LOC = RSLT_LOC, 
	APPL_USR_SRCH_PCT_CMPLT =PCT_CMPLT,
	APPL_USR_SRCH_ST_INDX = ST_INDX, 
	APPL_USR_SRCH_NUM_RSLTS = NUM_RSLTS, 
	APPL_USR_SRCH_CURR_ST_DT = CURR_ST, 
	APPL_USR_SRCH_CURR_END_DT = CURR_END, 
	APPL_USR_SRCH_ERR_MSG = ERR_MSG, 
	APPL_USR_SRCH_RETRY_CNT = RETRY_CNT,
    APPL_USR_SRCH_LN_ID = LN_ID,
    APPL_USR_SRCH_RNG_RSLTS = RNG_RSLTS,
    APPL_USR_SRCH_FILE_SIZE = FILE_SIZE,
	APPL_USR_SRCH_FILE_SIZE_CHECK_DT = FILE_CHECK,
	APPL_USR_SRCH_READY_TO_DWNLD = READY_FLAG,
    APPL_USR_SRCH_QUEUE_POS = QUEUE_POS,
    EMAILED = EMAILED
	WHERE APPL_USR_SRCH_SVD_NME = SRCH_NME AND APPL_USR_SRCH_REC_TRMN_DT IS NULL;
    
    ## 9/3/18 -- Experimental -- to attempt to fix the queue rotating logic that increases the queue position when a search completes
    SELECT APPL_LKUP_CD INTO CMPLT FROM APPL_LKUP WHERE APPL_LKUP_DESC IN ('Complete');
    SELECT APPL_LKUP_CD INTO INVLD FROM APPL_LKUP WHERE APPL_LKUP_DESC IN ('Invalid');
    
    IF STAT_CD = CMPLT OR STAT_CD = INVLD THEN
		UPDATE APPL_USR_SRCH_STAT SET APPL_USR_SRCH_QUEUE_POS = APPL_USR_SRCH_QUEUE_POS - 1			
		WHERE APPL_USR_SRCH_REC_TRMN_DT IS NULL
			AND APPL_USR_SRCH_STAT_CD IN 
				(SELECT APPL_LKUP_CD FROM APPL_LKUP WHERE APPL_LKUP_DESC IN ('Pending','Processing'))
			AND APPL_USR_SRCH_SVD_NME != SRCH_NME
			AND APPL_USR_SRCH_QUEUE_POS > QUEUE_POS
			AND NOT COALESCE(ON_HOLD, FALSE);
	END IF;

END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure p_get_zip_queue
-- -----------------------------------------------------

USE `lexis_nexis`;
DROP procedure IF EXISTS `lexis_nexis`.`p_get_zip_queue`;

DELIMITER $$
USE `lexis_nexis`$$
CREATE PROCEDURE `p_get_zip_queue`()
BEGIN
  SELECT S.APPL_USR_SRCH_STAT_REC_ID, S.APPL_USR_SRCH_SVD_NME, 
		S.APPL_USR_SRCH_STAT_CD, S.APPL_USR_SRCH_RSLT_LOC,	 S.APPL_USR_SRCH_QRY, 
        S.APPL_USR_SRCH_SRC, S.APPL_USR_SRCH_ST_DT, S.APPL_USR_SRCH_END_DT, 
        S.APPL_USR_SRCH_PCT_CMPLT, S.APPL_USR_SRCH_USR_NME, S.APPL_USR_SRCH_ST_INDX, 
        S.APPL_USR_SRCH_NUM_RSLTS, S.APPL_USR_SRCH_MTHD, S.APPL_USR_SRCH_CURR_ST_DT, 
        S.APPL_USR_SRCH_CURR_END_DT, S.APPL_USR_SRCH_RETRY_CNT, S.APPL_USR_SRCH_LN_ID, S.APPL_USR_SRCH_RNG_RSLTS,
        S.APPL_USR_SRCH_FILE_SIZE, S.APPL_USR_SRCH_FILE_SIZE_CHECK_DT, S.APPL_USR_SRCH_READY_TO_DWNLD, S.EMAILED
  FROM APPL_USR_SRCH_STAT S
  WHERE S.APPL_USR_SRCH_REC_TRMN_DT IS NULL
  AND S.APPL_USR_SRCH_RSLT_LOC IS NOT NULL AND S.APPL_USR_SRCH_RSLT_LOC != ""
  AND (S.APPL_USR_SRCH_READY_TO_DWNLD = FALSE OR S.APPL_USR_SRCH_READY_TO_DWNLD IS NULL)
  ORDER BY S.APPL_USR_SRCH_QUEUE_POS ASC;
 END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure p_del_usr_path
-- -----------------------------------------------------

USE `lexis_nexis`;
DROP procedure IF EXISTS `lexis_nexis`.`p_del_usr_path`;

DELIMITER $$
USE `lexis_nexis`$$
CREATE PROCEDURE `p_del_usr_path` (PATH VARCHAR(1000))
BEGIN
	UPDATE APPL_USR_SRCH_STAT
    SET APPL_USR_SRCH_RSLT_LOC = NULL
    WHERE APPL_USR_SRCH_RSLT_LOC = PATH
    AND APPL_USR_SRCH_USR_NME = 'DELETED' AND APPL_USR_SRCH_REC_TRMN_DT IS NOT NULL
    AND APPL_USR_SRCH_RSLT_LOC IS NOT NULL;
END
$$

DELIMITER ;
USE `lexis_nexis`;

DELIMITER $$

USE `lexis_nexis`$$
DROP TRIGGER IF EXISTS `lexis_nexis`.`APPL_RUN_STAT_AFTER_UPDATE` $$
USE `lexis_nexis`$$
CREATE DEFINER = CURRENT_USER TRIGGER `lexis_nexis`.`APPL_RUN_STAT_AFTER_UPDATE` AFTER UPDATE ON `APPL_RUN_STAT` FOR EACH ROW
BEGIN
	INSERT INTO APPL_RUN_STAT_HIST (JOB_TYP_CD, IS_RUNNING, MODIFIED_DT, TERMINATION_DT)
    VALUES (old.JOB_TYP_CD, old.IS_RUNNING, old.MODIFIED_DT, NOW());
END
$$


USE `lexis_nexis`$$
DROP TRIGGER IF EXISTS `lexis_nexis`.`APPL_RUN_STAT_AFTER_DELETE` $$
USE `lexis_nexis`$$
CREATE DEFINER = CURRENT_USER TRIGGER `lexis_nexis`.`APPL_RUN_STAT_AFTER_DELETE` AFTER DELETE ON `APPL_RUN_STAT` FOR EACH ROW
BEGIN
	INSERT INTO APPL_RUN_STAT_HIST (JOB_TYP_CD, IS_RUNNING, MODIFIED_DT, TERMINATION_DT)
    VALUES (old.JOB_TYP_CD, old.IS_RUNNING, old.MODIFIED_DT, NOW());
END
$$


USE `lexis_nexis`$$
DROP TRIGGER IF EXISTS `lexis_nexis`.`USR_SRCH_AFTER_UPDATE` $$
USE `lexis_nexis`$$
CREATE
TRIGGER `lexis_nexis`.`USR_SRCH_AFTER_UPDATE`
AFTER UPDATE ON `lexis_nexis`.`APPL_USR_SRCH_STAT`
FOR EACH ROW
BEGIN
INSERT INTO APPL_USR_SRCH_STAT_HIST (APPL_USR_SRCH_SVD_NME, APPL_USR_SRCH_USR_NME,
			APPL_USR_SRCH_STAT_CD, APPL_USR_SRCH_REC_EFF_DT, APPL_USR_SRCH_REC_TRMN_DT, APPL_USR_SRCH_RSLT_LOC, APPL_USR_SRCH_PCT_CMPLT, APPL_USR_SRCH_RNG_RSLTS, 
            APPL_USR_SRCH_QRY, APPL_USR_SRCH_SRC, APPL_USR_SRCH_ST_DT, APPL_USR_SRCH_END_DT, APPL_USR_SRCH_ST_INDX, APPL_USR_SRCH_NUM_RSLTS, APPL_USR_SRCH_MTHD, APPL_USR_SRCH_LN_ID,
            APPL_USR_SRCH_CURR_ST_DT, APPL_USR_SRCH_CURR_END_DT, APPL_USR_SRCH_ERR_MSG, APPL_USR_SRCH_RETRY_CNT, APPL_USR_SRCH_CREATE_DT, APPL_USR_SRCH_QUEUE_POS,
            APPL_USR_SRCH_FILE_SIZE, APPL_USR_SRCH_FILE_SIZE_CHECK_DT, APPL_USR_SRCH_READY_TO_DWNLD, ON_HOLD, EMAILED)
	VALUES (old.APPL_USR_SRCH_SVD_NME, old.APPL_USR_SRCH_USR_NME,
			old.APPL_USR_SRCH_STAT_CD, old.APPL_USR_SRCH_REC_EFF_DT, NOW(), old.APPL_USR_SRCH_RSLT_LOC, old.APPL_USR_SRCH_PCT_CMPLT, old.APPL_USR_SRCH_RNG_RSLTS, 
            old.APPL_USR_SRCH_QRY, old.APPL_USR_SRCH_SRC, old.APPL_USR_SRCH_ST_DT, old.APPL_USR_SRCH_END_DT, old.APPL_USR_SRCH_ST_INDX, old.APPL_USR_SRCH_NUM_RSLTS, old.APPL_USR_SRCH_MTHD, old.APPL_USR_SRCH_LN_ID,
            old.APPL_USR_SRCH_CURR_ST_DT, old.APPL_USR_SRCH_CURR_END_DT, old.APPL_USR_SRCH_ERR_MSG, old.APPL_USR_SRCH_RETRY_CNT, old.APPL_USR_SRCH_CREATE_DT, old.APPL_USR_SRCH_QUEUE_POS,
            old.APPL_USR_SRCH_FILE_SIZE, old.APPL_USR_SRCH_FILE_SIZE_CHECK_DT, old.APPL_USR_SRCH_READY_TO_DWNLD, old.ON_HOLD, old.EMAILED);
END$$


DELIMITER ;

SET SQL_MODE=@OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;
SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS;

-- -----------------------------------------------------
-- Data for table `lexis_nexis`.`APPL_LKUP`
-- -----------------------------------------------------
START TRANSACTION;
USE `lexis_nexis`;
INSERT INTO `lexis_nexis`.`APPL_LKUP` (`APPL_LKUP_REC_ID`, `APPL_LKUP_CD`, `APPL_LKUP_CATG_NME`, `APPL_LKUP_DESC`, `APPL_LKUP_REC_EFF_DT`) VALUES (1, 1, 'SRCH_STAT', 'Pending', now());
INSERT INTO `lexis_nexis`.`APPL_LKUP` (`APPL_LKUP_REC_ID`, `APPL_LKUP_CD`, `APPL_LKUP_CATG_NME`, `APPL_LKUP_DESC`, `APPL_LKUP_REC_EFF_DT`) VALUES (3, 2, 'SRCH_STAT', 'Processing', now());
INSERT INTO `lexis_nexis`.`APPL_LKUP` (`APPL_LKUP_REC_ID`, `APPL_LKUP_CD`, `APPL_LKUP_CATG_NME`, `APPL_LKUP_DESC`, `APPL_LKUP_REC_EFF_DT`) VALUES (5, 3, 'SRCH_STAT', 'Complete', now());
INSERT INTO `lexis_nexis`.`APPL_LKUP` (`APPL_LKUP_REC_ID`, `APPL_LKUP_CD`, `APPL_LKUP_CATG_NME`, `APPL_LKUP_DESC`, `APPL_LKUP_REC_EFF_DT`) VALUES (7, 4, 'SRCH_STAT', 'Invalid', now());
INSERT INTO `lexis_nexis`.`APPL_LKUP` (`APPL_LKUP_REC_ID`, `APPL_LKUP_CD`, `APPL_LKUP_CATG_NME`, `APPL_LKUP_DESC`, `APPL_LKUP_REC_EFF_DT`) VALUES (13, 100, 'LOG_CD', 'UI Error', now());
INSERT INTO `lexis_nexis`.`APPL_LKUP` (`APPL_LKUP_REC_ID`, `APPL_LKUP_CD`, `APPL_LKUP_CATG_NME`, `APPL_LKUP_DESC`, `APPL_LKUP_REC_EFF_DT`) VALUES (15, 101, 'LOG_CD', 'Web Service Error', now());
INSERT INTO `lexis_nexis`.`APPL_LKUP` (`APPL_LKUP_REC_ID`, `APPL_LKUP_CD`, `APPL_LKUP_CATG_NME`, `APPL_LKUP_DESC`, `APPL_LKUP_REC_EFF_DT`) VALUES (17, 102, 'LOG_CD', 'Database Error', now());
INSERT INTO `lexis_nexis`.`APPL_LKUP` (`APPL_LKUP_REC_ID`, `APPL_LKUP_CD`, `APPL_LKUP_CATG_NME`, `APPL_LKUP_DESC`, `APPL_LKUP_REC_EFF_DT`) VALUES (35, 280434, 'COMMON_SRC', 'All News, All Languages', now());
INSERT INTO `lexis_nexis`.`APPL_LKUP` (`APPL_LKUP_REC_ID`, `APPL_LKUP_CD`, `APPL_LKUP_CATG_NME`, `APPL_LKUP_DESC`, `APPL_LKUP_REC_EFF_DT`) VALUES (23, 8399, 'COMMON_SRC', 'All English Language News', now());
INSERT INTO `lexis_nexis`.`APPL_LKUP` (`APPL_LKUP_REC_ID`, `APPL_LKUP_CD`, `APPL_LKUP_CATG_NME`, `APPL_LKUP_DESC`, `APPL_LKUP_REC_EFF_DT`) VALUES (29, 161887, 'COMMON_SRC', 'English Language News - Most Recent 90 Days', now());
INSERT INTO `lexis_nexis`.`APPL_LKUP` (`APPL_LKUP_REC_ID`, `APPL_LKUP_CD`, `APPL_LKUP_CATG_NME`, `APPL_LKUP_DESC`, `APPL_LKUP_REC_EFF_DT`) VALUES (27, 148107, 'COMMON_SRC', 'Non-English Language News', now());
INSERT INTO `lexis_nexis`.`APPL_LKUP` (`APPL_LKUP_REC_ID`, `APPL_LKUP_CD`, `APPL_LKUP_CATG_NME`, `APPL_LKUP_DESC`, `APPL_LKUP_REC_EFF_DT`) VALUES (31, 232166, 'COMMON_SRC', 'Non-English Language News - Most Recent 90 Days', now());
INSERT INTO `lexis_nexis`.`APPL_LKUP` (`APPL_LKUP_REC_ID`, `APPL_LKUP_CD`, `APPL_LKUP_CATG_NME`, `APPL_LKUP_DESC`, `APPL_LKUP_REC_EFF_DT`) VALUES (25, 8422, 'COMMON_SRC', 'Major Newspapers', now());
INSERT INTO `lexis_nexis`.`APPL_LKUP` (`APPL_LKUP_REC_ID`, `APPL_LKUP_CD`, `APPL_LKUP_CATG_NME`, `APPL_LKUP_DESC`, `APPL_LKUP_REC_EFF_DT`) VALUES (37, 307555, 'COMMON_SRC', 'Major Non-US Newspapers', now());
INSERT INTO `lexis_nexis`.`APPL_LKUP` (`APPL_LKUP_REC_ID`, `APPL_LKUP_CD`, `APPL_LKUP_CATG_NME`, `APPL_LKUP_DESC`, `APPL_LKUP_REC_EFF_DT`) VALUES (39, 307574, 'COMMON_SRC', 'Major US Newspapers', now());
INSERT INTO `lexis_nexis`.`APPL_LKUP` (`APPL_LKUP_REC_ID`, `APPL_LKUP_CD`, `APPL_LKUP_CATG_NME`, `APPL_LKUP_DESC`, `APPL_LKUP_REC_EFF_DT`) VALUES (33, 238672, 'COMMON_SRC', 'Major World Newspapers', now());
INSERT INTO `lexis_nexis`.`APPL_LKUP` (`APPL_LKUP_REC_ID`, `APPL_LKUP_CD`, `APPL_LKUP_CATG_NME`, `APPL_LKUP_DESC`, `APPL_LKUP_REC_EFF_DT`) VALUES (19, 103, 'LOG_CD', 'Web Service Call', now());
INSERT INTO `lexis_nexis`.`APPL_LKUP` (`APPL_LKUP_REC_ID`, `APPL_LKUP_CD`, `APPL_LKUP_CATG_NME`, `APPL_LKUP_DESC`, `APPL_LKUP_REC_EFF_DT`) VALUES (21, 104, 'LOG_CD', 'UI Call', now());
INSERT INTO `lexis_nexis`.`APPL_LKUP` (`APPL_LKUP_REC_ID`, `APPL_LKUP_CD`, `APPL_LKUP_CATG_NME`, `APPL_LKUP_DESC`, `APPL_LKUP_REC_EFF_DT`) VALUES (9, 10, 'JOB_TYP', 'Queue Processor', now());
INSERT INTO `lexis_nexis`.`APPL_LKUP` (`APPL_LKUP_REC_ID`, `APPL_LKUP_CD`, `APPL_LKUP_CATG_NME`, `APPL_LKUP_DESC`, `APPL_LKUP_REC_EFF_DT`) VALUES (11, 11, 'JOB_TYP', 'Zip Processor', now());
INSERT INTO `lexis_nexis`.`APPL_LKUP` (`APPL_LKUP_REC_ID`, `APPL_LKUP_CD`, `APPL_LKUP_CATG_NME`, `APPL_LKUP_DESC`, `APPL_LKUP_REC_EFF_DT`) VALUES (41, 105, 'LOG_CD', 'Non Critical Error', now());

COMMIT;


-- -----------------------------------------------------
-- Data for table `lexis_nexis`.`APPL_PARAM`
-- -----------------------------------------------------
START TRANSACTION;
USE `lexis_nexis`;
INSERT INTO `lexis_nexis`.`APPL_PARAM` (`APPL_PARAM_REC_ID`, `APPL_PARAM_NME`, `APPL_PARAM_VAL`, `APPL_PARAM_DESC`, `APPL_PARAM_EFF_DT`) VALUES (1, 'SRCH_PR_HR', '499', 'Number of allowed searches per hour', now());
INSERT INTO `lexis_nexis`.`APPL_PARAM` (`APPL_PARAM_REC_ID`, `APPL_PARAM_NME`, `APPL_PARAM_VAL`, `APPL_PARAM_DESC`, `APPL_PARAM_EFF_DT`) VALUES (3, 'RSLT_PR_SRCH', '10', 'Number of allowed results to be returned per search', now());
INSERT INTO `lexis_nexis`.`APPL_PARAM` (`APPL_PARAM_REC_ID`, `APPL_PARAM_NME`, `APPL_PARAM_VAL`, `APPL_PARAM_DESC`, `APPL_PARAM_EFF_DT`) VALUES (5, 'DOC_PR_SRCH', '10', 'Number of documents allowed per search', now());
INSERT INTO `lexis_nexis`.`APPL_PARAM` (`APPL_PARAM_REC_ID`, `APPL_PARAM_NME`, `APPL_PARAM_VAL`, `APPL_PARAM_DESC`, `APPL_PARAM_EFF_DT`) VALUES (7, 'PRJ_ID', '8412', 'Project ID to use for the web service', now());
INSERT INTO `lexis_nexis`.`APPL_PARAM` (`APPL_PARAM_REC_ID`, `APPL_PARAM_NME`, `APPL_PARAM_VAL`, `APPL_PARAM_DESC`, `APPL_PARAM_EFF_DT`) VALUES (9, 'RSLT_PR_ADHOC_SRCH', '40', 'Number of allowed results to be returned per adhoc search', now());
INSERT INTO `lexis_nexis`.`APPL_PARAM` (`APPL_PARAM_REC_ID`, `APPL_PARAM_NME`, `APPL_PARAM_VAL`, `APPL_PARAM_DESC`, `APPL_PARAM_EFF_DT`) VALUES (11, 'WEEKDAY_START', '23:00', 'Processing start time for a weekday', now());
INSERT INTO `lexis_nexis`.`APPL_PARAM` (`APPL_PARAM_REC_ID`, `APPL_PARAM_NME`, `APPL_PARAM_VAL`, `APPL_PARAM_DESC`, `APPL_PARAM_EFF_DT`) VALUES (13, 'WEEKDAY_END', '06:00', 'Processing end time for a weekday', now());
INSERT INTO `lexis_nexis`.`APPL_PARAM` (`APPL_PARAM_REC_ID`, `APPL_PARAM_NME`, `APPL_PARAM_VAL`, `APPL_PARAM_DESC`, `APPL_PARAM_EFF_DT`) VALUES (15, 'WEEKEND_START', '0:00', 'Processing start time for a weekend', now());
INSERT INTO `lexis_nexis`.`APPL_PARAM` (`APPL_PARAM_REC_ID`, `APPL_PARAM_NME`, `APPL_PARAM_VAL`, `APPL_PARAM_DESC`, `APPL_PARAM_EFF_DT`) VALUES (17, 'WEEKEND_END', '0:00', 'Processing end time for a weekend', now());
INSERT INTO `lexis_nexis`.`APPL_PARAM` (`APPL_PARAM_REC_ID`, `APPL_PARAM_NME`, `APPL_PARAM_VAL`, `APPL_PARAM_DESC`, `APPL_PARAM_EFF_DT`) VALUES (19, 'ADHOC_SRCH_PER_SESSION', '10', 'Number of allowed adhoc searches to be performed per session', now());
INSERT INTO `lexis_nexis`.`APPL_PARAM` (`APPL_PARAM_REC_ID`, `APPL_PARAM_NME`, `APPL_PARAM_VAL`, `APPL_PARAM_DESC`, `APPL_PARAM_EFF_DT`) VALUES (21, 'MTHS_TO_DEL', '3', 'Number of months to retain searches before deleting', now());
INSERT INTO `lexis_nexis`.`APPL_PARAM` (`APPL_PARAM_REC_ID`, `APPL_PARAM_NME`, `APPL_PARAM_VAL`, `APPL_PARAM_DESC`, `APPL_PARAM_EFF_DT`) VALUES (23, 'MAX_RETRY', '3', 'Max tries per search if it fails', now());
INSERT INTO `lexis_nexis`.`APPL_PARAM` (`APPL_PARAM_REC_ID`, `APPL_PARAM_NME`, `APPL_PARAM_VAL`, `APPL_PARAM_DESC`, `APPL_PARAM_EFF_DT`) VALUES (25, 'BEGIN_DT', '1/1/1980', 'Begin date to use if none is provided', now());
INSERT INTO `lexis_nexis`.`APPL_PARAM` (`APPL_PARAM_REC_ID`, `APPL_PARAM_NME`, `APPL_PARAM_VAL`, `APPL_PARAM_DESC`, `APPL_PARAM_EFF_DT`) VALUES (27, 'WS_TRACE_LOGGING', '1', '1/0: If logging of all web service calls is enabled', now());

COMMIT;


-- -----------------------------------------------------
-- Data for table `lexis_nexis`.`APPL_SRCH_STAT`
-- -----------------------------------------------------
START TRANSACTION;
USE `lexis_nexis`;
INSERT INTO `lexis_nexis`.`APPL_SRCH_STAT` (`APPL_SRCH_STAT_REC_ID`, `APPL_SRCH_STAT_VAL`, `APPL_SRCH_STAT_REC_EFF_DT`, `APPL_SRCH_STAT_REC_LAST_UPD_DT`, `APPL_SRCH_STAT_REC_TRMN_DT`) VALUES (1, 0, now(), now(), NULL);

COMMIT;

